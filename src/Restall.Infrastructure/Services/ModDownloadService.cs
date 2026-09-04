using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

internal sealed partial class ModDownloadService : IModDownloadService
{
    private const string s_reShadeStartUrl = "https://reshade.me/downloads/ReShade_Setup_";
    private const string s_reShadeEndUrl = "_Addon.exe";

    private const string s_renoDXSnapshotDownloadBaseUrl =
        "https://github.com/clshortfuse/renodx/releases/download/snapshot/";

    private const string s_renoDXNightlyDownloadBaseUrl = "https://github.com/clshortfuse/renodx/releases/download/";
    private const string s_renoDXUnityDownloadBaseUrl = "https://notvoosh.github.io/renodx-unity/";

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_downloadLocks = new();
    private readonly HttpClient _httpClient;
    private readonly ILogger<ModDownloadService> _logger;
    private readonly IPathService _pathService;

    public ModDownloadService(
        ILogger<ModDownloadService> logger,
        HttpClient httpClient,
        IPathService pathService
    )
    {
        _logger = logger;
        _httpClient = httpClient;
        _pathService = pathService;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Restall");
    }

    // TODO: rename methods in this class
    public async Task<Result> DownloadRenoDXAsync(RenoDX.Branch branch, string? addonFileName = null,
        string? version = null, string? wikiSnapshotUrl = null, IProgress<DownloadProgressReportDto>? progress = null)
    {
        string downloadUrl;
        string fileName;

        switch (branch)
        {
            case RenoDX.Branch.Wiki:
                if (string.IsNullOrWhiteSpace(wikiSnapshotUrl))
                    return Result.Error("RenoDX wiki branch requires a wiki snapshot URL.");

                downloadUrl = wikiSnapshotUrl;
                fileName = Path.GetFileName(new Uri(wikiSnapshotUrl).AbsolutePath);
                break;
            case RenoDX.Branch.Snapshot:
                if (string.IsNullOrWhiteSpace(addonFileName))
                    return Result.Error("RenoDX snapshot branch requires a filename to download.");

                downloadUrl = $"{s_renoDXSnapshotDownloadBaseUrl}{addonFileName}";
                fileName = addonFileName;
                break;
            case RenoDX.Branch.Nightly:
                if (string.IsNullOrWhiteSpace(addonFileName) || string.IsNullOrWhiteSpace(version))
                    return Result.Error("RenoDX nightly branch requires both addon filename and version.");

                downloadUrl = $"{s_renoDXNightlyDownloadBaseUrl}nightly-{version}/{addonFileName}";
                fileName = addonFileName;
                break;
            default:
                return Result.Error($"Branch {branch} does not support automated downloads.");
        }

        var cacheDir = _pathService.GetRenoDXDownloadCacheDirectory(branch);
        return await DownloadFileAsync(downloadUrl, cacheDir, fileName, progress);
    }

    public async Task<Result> DownloadUnityRenoDXAsync(string addonFileName,
        IProgress<DownloadProgressReportDto>? progress = null)
    {
        var downloadUrl = s_renoDXUnityDownloadBaseUrl + addonFileName;
        var cacheDir = _pathService.GetRenoDXDownloadCacheDirectory(RenoDX.Branch.Wiki);
        return await DownloadFileAsync(downloadUrl, cacheDir, addonFileName, progress);
    }

    public async Task<Result> DownloadReShadeAsync(ReShade.Branch branch, string version,
        IProgress<DownloadProgressReportDto>? progress = null)
    {
        var downloadUrl = $"{s_reShadeStartUrl}{version}{s_reShadeEndUrl}";
        var installerPath = _pathService.GetReShadeInstallerFilePath(branch, version);

        return await DownloadFileAsync(downloadUrl, Path.GetDirectoryName(installerPath)!,
            Path.GetFileName(installerPath), progress);
    }

    private async Task<Result> DownloadFileAsync(string url, string destinationDirectory, string filename,
        IProgress<DownloadProgressReportDto>? progress)
    {
        try
        {
            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Error("Permission denied creating download directory.", ErrorType.PermissionDenied, ex);
        }
        catch (IOException ex)
        {
            return Result.Error("Failed to create download directory.", ErrorType.FileSystemError, ex);
        }

        var destinationPath = Path.Combine(destinationDirectory, filename);
        var fileLock = s_downloadLocks.GetOrAdd(destinationPath, _ => new SemaphoreSlim(1, 1));

        await fileLock.WaitAsync();

        try
        {
            if (File.Exists(destinationPath))
            {
                LogFileAlreadyExists(filename);
                progress?.Report(new DownloadProgressReportDto(filename, 100));
                return Result.Success();
            }

            return await PerformDownloadAsync(url, destinationDirectory, destinationPath, filename, progress);
        }
        finally
        {
            fileLock.Release();
            if (fileLock.CurrentCount == 1 && s_downloadLocks.TryRemove(destinationPath, out var removed))
                removed.Dispose();
        }
    }

    private async Task<Result> PerformDownloadAsync(string url, string destinationDirectory, string destinationPath,
        string filename,
        IProgress<DownloadProgressReportDto>? progress)
    {
        try
        {
            LogFileDownloadStart(filename, destinationDirectory, url);

            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write,
                FileShare.None, 8192, true);

            var lastReportedPercent = -1;
            var buffer = new byte[8192];
            long bytesReceived = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                bytesReceived += bytesRead;

                var percent = totalBytes is > 0
                    ? (int)(bytesReceived * 100 / totalBytes.Value)
                    : -1;

                if (percent != lastReportedPercent)
                {
                    progress?.Report(new DownloadProgressReportDto(filename, percent));
                    lastReportedPercent = percent;
                }
            }

            LogFileDownloadSuccess(filename, destinationDirectory);

            return Result.Success();
        }
        // TODO: handle mod download failures with cleanups
        catch (TaskCanceledException ex)
        {
            progress?.Report(new DownloadProgressReportDto(filename, -1));
            return Result.Error($"Download timed out for {filename} from {url}", ErrorType.NetworkTimeout, ex);
        }
        catch (HttpRequestException ex)
        {
            return Result.Error($"Server error downloading {filename}. ({(int?)ex.StatusCode}): {url}",
                ErrorType.DownloadFailed, ex);
        }
        catch (IOException ex)
        {
            return Result.Error($"Disk write failed for {filename}. Disk may be full or path locked.",
                ErrorType.FileSystemError, ex);
        }
        catch (Exception ex)
        {
            if (File.Exists(destinationPath))
                try
                {
                    File.Delete(destinationPath);
                }
                catch (Exception cleanupEx)
                {
                    LogPartialDownloadCleanupFailure(destinationPath, cleanupEx);
                }

            return Result.Error($"Failed to download {filename} from {url}", ErrorType.Unknown, ex);
        }
    }
}