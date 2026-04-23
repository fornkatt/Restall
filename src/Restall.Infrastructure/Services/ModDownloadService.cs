using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using System.Collections.Concurrent;
using Restall.Application.Common;

namespace Restall.Infrastructure.Services;

internal sealed class ModDownloadService : IModDownloadService
{
    private const string s_reShadeStartUrl = "https://reshade.me/downloads/ReShade_Setup_";
    private const string s_reShadeEndUrl = "_Addon.exe";
    private const string s_renoDXSnapshotDownloadBaseUrl = "https://github.com/clshortfuse/renodx/releases/download/snapshot/";
    private const string s_renoDXNightlyDownloadBaseUrl = "https://github.com/clshortfuse/renodx/releases/download/";
    private const string s_renoDXUnityDownloadBaseUrl = "https://notvoosh.github.io/renodx-unity/";

    private readonly HttpClient _httpClient;
    private readonly IPathService _pathService;
    private readonly ILogService _logService;

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_downloadLocks = new(); 

    public ModDownloadService(
        HttpClient httpClient,
        IPathService pathService,
        ILogService logService
        )
    {
        _httpClient = httpClient;
        _pathService = pathService;
        _logService = logService;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Restall");
    }
    
    public async Task<Result> DownloadRenoDXAsync(RenoDX.Branch branch, string? addonFileName = null,
        string? version = null, string? wikiSnapshotUrl = null, IProgress<DownloadProgressReportDto>? progress = null)
    {
        string downloadUrl;
        string fileName;

        switch (branch)
        {
            case RenoDX.Branch.Wiki:
                if (string.IsNullOrWhiteSpace(wikiSnapshotUrl))
                {
                    return Result.Err("RenoDX wiki branch requires a wiki snapshot URL.");
                }
                downloadUrl = wikiSnapshotUrl;
                fileName = Path.GetFileName(new Uri(wikiSnapshotUrl).AbsolutePath);
                break;
            case RenoDX.Branch.Snapshot:
                if (string.IsNullOrWhiteSpace(addonFileName))
                {
                    return Result.Err("RenoDX snapshot branch requires a filename to download.");
                }
                downloadUrl = $"{s_renoDXSnapshotDownloadBaseUrl}{addonFileName}";
                fileName = addonFileName;
                break;
            case RenoDX.Branch.Nightly:
                if (string.IsNullOrWhiteSpace(addonFileName) || string.IsNullOrWhiteSpace(version))
                {
                    return Result.Err("RenoDX nightly branch requires both addon filename and version.");
                }
                downloadUrl = $"{s_renoDXNightlyDownloadBaseUrl}nightly-{version}/{addonFileName}";
                fileName = addonFileName;
                break;
            default:
                return Result.Err($"Branch {branch} does not support automated downloads.");
        }
        var cacheDir = _pathService.GetRenoDXDownloadCachePath(branch);
        return await DownloadFileAsync(downloadUrl, cacheDir, fileName, progress);
    }

    public async Task<Result> DownloadUnityRenoDXAsync(string addonFileName, IProgress<DownloadProgressReportDto>? progress = null)
    {
        var downloadUrl = s_renoDXUnityDownloadBaseUrl + addonFileName;
        var cacheDir = _pathService.GetRenoDXDownloadCachePath(RenoDX.Branch.Snapshot);
        return await DownloadFileAsync(downloadUrl, cacheDir, addonFileName, progress);
    }

    public async Task<Result> DownloadReShadeAsync(ReShade.Branch branch, string version, IProgress<DownloadProgressReportDto>? progress = null)
    {
        var downloadUrl = $"{s_reShadeStartUrl}{version}{s_reShadeEndUrl}";
        var installerPath = _pathService.GetReShadeInstallerFilePath(branch, version);

        return await DownloadFileAsync(downloadUrl, Path.GetDirectoryName(installerPath)!, Path.GetFileName(installerPath), progress);
    }

    private async Task<Result> DownloadFileAsync(string url, string directory, string fileName, IProgress<DownloadProgressReportDto>? progress)
    {
        try
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Err("Permission denied creating download directory.", ResultError.PermissionDenied, ex);
        }
        catch (IOException ex)
        {
            return Result.Err("Failed to create download directory.", ResultError.FileSystemError, ex);
        }
        
        var destinationPath = Path.Combine(directory, fileName);
        var fileLock = s_downloadLocks.GetOrAdd(destinationPath, _ => new SemaphoreSlim(1, 1));

        await fileLock.WaitAsync();

        try
        {
            if (File.Exists(destinationPath))
            {
                await _logService.LogInfoAsync($"{fileName} already downloaded by another task, skipping.");
                progress?.Report(new DownloadProgressReportDto(fileName, 100));
                return Result.Ok();
            }

            return await PerformDownloadAsync(url, destinationPath, fileName, progress);
        }
        finally
        {
            fileLock.Release();
            if (fileLock.CurrentCount == 1 && s_downloadLocks.TryRemove(destinationPath, out var removed))
                removed.Dispose();
        }
    }

    private async Task<Result> PerformDownloadAsync(string url, string destinationPath, string filename, IProgress<DownloadProgressReportDto>? progress)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true);

            int lastReportedPercent = -1;
            var buffer = new byte[8192];
            long bytesReceived = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                bytesReceived += bytesRead;

                int percent = totalBytes is > 0
                    ? (int)(bytesReceived * 100 / totalBytes.Value)
                    : -1;

                if (percent != lastReportedPercent)
                {
                    progress?.Report(new DownloadProgressReportDto(filename, percent));
                    lastReportedPercent = percent;
                }
            }

            await _logService.LogInfoAsync($"Successfully downloaded {filename} to {Path.GetDirectoryName(destinationPath)}");
            return Result.Ok();
        }
        catch (TaskCanceledException ex)
        {
            progress?.Report(new DownloadProgressReportDto(filename, -1));
            return Result.Err($"Download timed out for {filename} from {url}", ResultError.NetworkTimeout, ex);
        }
        catch (HttpRequestException ex)
        {
            return Result.Err($"Server error downloading {filename}. ({(int?)ex.StatusCode}): {url}", ResultError.DownloadFailed, ex);
        }
        catch (IOException ex)
        {
            return Result.Err($"Disk write failed for {filename}. Disk may be full or path locked.", ResultError.FileSystemError, ex);
        }
        catch (Exception ex)
        {
            if (File.Exists(destinationPath))
            {
                try { File.Delete(destinationPath); }
                catch (Exception cleanupEx)
                {
                    await _logService.LogErrorAsync($"Could not cleanup partial download at {destinationPath}", cleanupEx);
                }
            }
            return Result.Err($"Failed to download {filename} from {url}", ResultError.Unknown, ex);
        }
    }
}