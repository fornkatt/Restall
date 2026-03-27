using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using System.Collections.Concurrent;

namespace Restall.Infrastructure.Services;

internal sealed class ModDownloadService : IModDownloadService
{
    private const string s_reShadeStartUrl = "https://reshade.me/downloads/ReShade_Setup_";
    private const string s_reShadeEndUrl = "_Addon.exe";
    private const string s_renoDXSnapshotDownloadBaseUrl = "https://github.com/clshortfuse/renodx/releases/download/snapshot/";
    private const string s_renoDXNightlyDownloadBaseUrl = "https://github.com/clshortfuse/renodx/releases/download/";
    private const string s_renoDXUnityDownloadBaseUrl = "https://notvoosh.github.io/renodx-unity/";

    private readonly HttpClient _httpClient;
    private readonly ICachePathService _cachePathService;
    private readonly ILogService _logService;

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_downloadLocks = new(); 

    public ModDownloadService(
        HttpClient httpClient,
        ICachePathService cachePathService,
        ILogService logService
        )
    {
        _httpClient = httpClient;
        _cachePathService = cachePathService;
        _logService = logService;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Restall");
    }

    // We only handle donwloads from GitHub since Nexus and Discord are problematic. We provide links in the UI for manual download.
    // We need the full download URL stored in the DTO for the wiki branch since they are downloaded from the maintainer's GitHub
    public async Task<bool> DownloadRenoDXAsync(RenoDX.Branch branch, string? addonFileName = null,
        string? version = null, string? wikiSnapshotUrl = null, IProgress<DownloadProgressReportDto>? progress = null)
    {
        string downloadUrl;
        string fileName;

        switch (branch)
        {
            case RenoDX.Branch.Wiki:
                if (string.IsNullOrWhiteSpace(wikiSnapshotUrl))
                {
                    await _logService.LogWarningAsync("RenoDX wiki branch requires a wiki snapshot URL");
                    return false;
                }
                downloadUrl = wikiSnapshotUrl;
                fileName = Path.GetFileName(new Uri(wikiSnapshotUrl).AbsolutePath);
                break;
            case RenoDX.Branch.Snapshot:
                if (string.IsNullOrWhiteSpace(addonFileName))
                {
                    await _logService.LogWarningAsync("RenoDX snapshot branch requires a filename to download.");
                    return false;
                }
                downloadUrl = $"{s_renoDXSnapshotDownloadBaseUrl}{addonFileName}";
                fileName = addonFileName;
                break;
            case RenoDX.Branch.Nightly:
                if (string.IsNullOrWhiteSpace(addonFileName) || string.IsNullOrWhiteSpace(version))
                {
                    await _logService.LogWarningAsync("RenoDX nightly branch requires both addon filename and version.");
                    return false;
                }
                downloadUrl = $"{s_renoDXNightlyDownloadBaseUrl}nightly-{version}/{addonFileName}";
                fileName = addonFileName;
                break;
            default:
                await _logService.LogWarningAsync($"Branch {branch} does not support automated downloads.");
                return false;
        }

        var cacheDir = _cachePathService.GetRenoDXDownloadCachePath(branch);
        return await DownloadFileAsync(downloadUrl, cacheDir, fileName, progress);
    }

    public async Task<bool> DownloadUnityRenoDXAsync(string addonFileName, IProgress<DownloadProgressReportDto>? progress = null)
    {
        var downloadUrl = s_renoDXUnityDownloadBaseUrl + addonFileName;
        var cacheDir = _cachePathService.GetRenoDXDownloadCachePath(RenoDX.Branch.Snapshot);
        return await DownloadFileAsync(downloadUrl, cacheDir, addonFileName, progress);
    }

    public async Task<bool> DownloadReShadeAsync(ReShade.Branch branch, string version, IProgress<DownloadProgressReportDto>? progress = null)
    {
        var downloadUrl = $"{s_reShadeStartUrl}{version}{s_reShadeEndUrl}";
        var installerPath = _cachePathService.GetReShadeInstallerFilePath(branch, version);

        return await DownloadFileAsync(downloadUrl, Path.GetDirectoryName(installerPath)!, Path.GetFileName(installerPath), progress);
    }

    private async Task<bool> DownloadFileAsync(string url, string directory, string fileName, IProgress<DownloadProgressReportDto>? progress)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var destinationPath = Path.Combine(directory, fileName);

        var fileLock = s_downloadLocks.GetOrAdd(destinationPath, _ => new SemaphoreSlim(1, 1));

        await fileLock.WaitAsync();

        try
        {
            if (File.Exists(destinationPath))
            {
                await _logService.LogInfoAsync($"{fileName} already downloaded by another task, skipping.");
                progress?.Report(new DownloadProgressReportDto(fileName, 100));
                return true;
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

    private async Task<bool> PerformDownloadAsync(string url, string destinationPath, string filename, IProgress<DownloadProgressReportDto>? progress)
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
            return true;
        }
        catch (TaskCanceledException ex)
        {
            await _logService.LogErrorAsync($"Download timed out for {filename} from {url}", ex);
            progress?.Report(new DownloadProgressReportDto(filename, -1));
            return false;
        }
        catch (HttpRequestException ex)
        {
            await _logService.LogErrorAsync($"Server error downloading {filename}. ({(int?)ex.StatusCode}): {url}", ex);
            return false;
        }
        catch (IOException ex)
        {
            await _logService.LogErrorAsync($"Disk write failed for {filename}. Disk may be full or path locked.", ex);
            return false;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to download {filename} from {url}", ex);

            if (File.Exists(destinationPath))
            {
                try { File.Delete(destinationPath); }
                catch (Exception cleanupEx)
                {
                    await _logService.LogErrorAsync($"Could not cleanup partial download at {destinationPath}", cleanupEx);
                }
            }

            return false;
        }
    }
}