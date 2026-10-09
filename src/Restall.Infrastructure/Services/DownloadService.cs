// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using System.Collections.Concurrent;

namespace Restall.Infrastructure.Services;

internal sealed partial class DownloadService : IDownloadService
{
    internal const string HttpClientName = nameof(DownloadService);

    private const string ReShadeStartUrl = "https://reshade.me/downloads/ReShade_Setup_";
    private const string ReShadeEndUrl = "_Addon.exe";

    private const string RenoDXSnapshotDownloadBaseUrl =
        "https://github.com/clshortfuse/renodx/releases/download/snapshot/";

    private const string RenoDXNightlyDownloadBaseUrl = "https://github.com/clshortfuse/renodx/releases/download/";
    private const string RenoDXUnityDownloadBaseUrl = "https://notvoosh.github.io/renodx-unity/";
    private const string RenoDXUEExtendedDownloadBaseUrl = "https://marat569.github.io/renodx/";

    private readonly Dictionary<RenoDXWikiModType, string> _externalHostBaseUrls = new()
    {
        [RenoDXWikiModType.Unity] = RenoDXUnityDownloadBaseUrl,
        [RenoDXWikiModType.UnrealExtended] = RenoDXUEExtendedDownloadBaseUrl
    };

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_downloadLocks = new();
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<DownloadService> _logger;
    private readonly IPathService _pathService;

    internal TimeSpan StallTimeout { get; init; } = TimeSpan.FromSeconds(30);

    public DownloadService(
        ILogger<DownloadService> logger,
        IHttpClientFactory clientFactory,
        IPathService pathService
    )
    {
        _logger = logger;
        _clientFactory = clientFactory;
        _pathService = pathService;
    }

    public async Task<Result> DownloadAsync(Uri url, string destinationPath,
        IProgress<DownloadProgressReport>? progress = null, CancellationToken cancellationToken = default)
    {
        var filename = Path.GetFileName(destinationPath);
        var destinationDirectory = Path.GetDirectoryName(destinationPath);

        if (string.IsNullOrWhiteSpace(destinationDirectory))
            throw new ArgumentException("The destination path must include a directory", nameof(destinationPath));

        var tempPath = $"{destinationPath}.{Path.GetRandomFileName()}.part";

        LogFileDownloadStart(filename, destinationDirectory, url.AbsoluteUri);

        try
        {
            Directory.CreateDirectory(destinationDirectory);
            await DownloadToFileAsync(url, tempPath, filename, progress, cancellationToken);
            File.Move(tempPath, destinationPath, overwrite: true);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Error($"Download of \"{filename}\" from \"{url}\" timed out or stalled",
                ErrorType.NetworkTimeout, ex);
        }
        catch (HttpRequestException ex)
        {
            return Result.Error($"Failed to download \"{filename}\" from \"{url}\" ({(int?)ex.StatusCode})",
                ErrorType.DownloadFailed, ex);
        }
        catch (HttpIOException ex)
        {
            return Result.Error($"The connection to \"{url}\" broke while downloading \"{filename}\"",
                ErrorType.DownloadFailed, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Error($"Permission denied while writing \"{destinationPath}\"", ErrorType.PermissionDenied,
                ex);
        }
        catch (IOException ex)
        {
            return Result.Error($"Failed to write \"{destinationPath}\"", ErrorType.FileSystemError, ex);
        }
        finally
        {
            TryDeleteTempFile(tempPath);
        }

        LogFileDownloadSuccess(filename, destinationDirectory);

        return Result.Success();
    }

    // TODO: rename methods in this class
    public async Task<Result> DownloadRenoDXAsync(RenoDX.Branch branch, string? addonFileName = null,
        string? version = null, string? wikiSnapshotUrl = null, IProgress<DownloadProgressReport>? progress = null)
    {
        string downloadUrl;
        string fileName;

        switch (branch)
        {
            case RenoDX.Branch.Direct:
                if (string.IsNullOrWhiteSpace(wikiSnapshotUrl))
                    return Result.Error("RenoDX wiki branch requires a wiki snapshot URL.");

                downloadUrl = wikiSnapshotUrl;
                fileName = Path.GetFileName(new Uri(wikiSnapshotUrl).AbsolutePath);
                break;
            case RenoDX.Branch.Snapshot:
                if (string.IsNullOrWhiteSpace(addonFileName))
                    return Result.Error("RenoDX snapshot branch requires a filename to download.");

                downloadUrl = $"{RenoDXSnapshotDownloadBaseUrl}{addonFileName}";
                fileName = addonFileName;
                break;
            case RenoDX.Branch.Nightly:
                if (string.IsNullOrWhiteSpace(addonFileName) || string.IsNullOrWhiteSpace(version))
                    return Result.Error("RenoDX nightly branch requires both addon filename and version.");

                downloadUrl = $"{RenoDXNightlyDownloadBaseUrl}nightly-{version}/{addonFileName}";
                fileName = addonFileName;
                break;
            default:
                return Result.Error($"Branch {branch} does not support automated downloads.");
        }

        var cacheDir = _pathService.GetRenoDXDownloadCacheDirectory(branch);
        return await DownloadFileAsync(downloadUrl, cacheDir, fileName, progress);
    }

    public async Task<Result> DownloadExternalRenoDXAsync(RenoDXWikiModType renoDxWikiModType, string addonFileName,
        IProgress<DownloadProgressReport>? progress = null)
    {
        if (!_externalHostBaseUrls.TryGetValue(renoDxWikiModType, out var baseUrl))
            return Result.Error($"{renoDxWikiModType} does not have and externally hosted RenoDX download configured.");

        var downloadUrl = baseUrl + addonFileName;
        var cacheDir = _pathService.GetRenoDXDownloadCacheDirectory(RenoDX.Branch.Direct);
        return await DownloadFileAsync(downloadUrl, cacheDir, addonFileName, progress);
    }

    public async Task<Result> DownloadReShadeAsync(ReShade.Branch branch, string version,
        IProgress<DownloadProgressReport>? progress = null)
    {
        var downloadUrl = $"{ReShadeStartUrl}{version}{ReShadeEndUrl}";
        var installerPath = _pathService.GetReShadeInstallerFilePath(branch, version);

        return await DownloadFileAsync(downloadUrl, Path.GetDirectoryName(installerPath)!,
            Path.GetFileName(installerPath), progress);
    }

    private async Task DownloadToFileAsync(Uri url, string tempPath, string filename,
        IProgress<DownloadProgressReport>? progress, CancellationToken cancellationToken = default)
    {
        const int bufferSize = 81920;

        using var stallTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        stallTimeout.CancelAfter(StallTimeout);
        var httpClient = _clientFactory.CreateClient(HttpClientName);

        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead,
            stallTimeout.Token);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;

        await using var contentStream = await response.Content.ReadAsStreamAsync(stallTimeout.Token);
        await using var fileStream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            bufferSize, useAsync: true);

        var buffer = new byte[bufferSize];
        var lastReportedPercent = -1;
        long bytesReceived = 0;
        var bytesRead = 0;

        while ((bytesRead = await contentStream.ReadAsync(buffer, stallTimeout.Token)) > 0)
        {
            stallTimeout.CancelAfter(StallTimeout);
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            bytesReceived += bytesRead;

            var percent = totalBytes is > 0
                ? (int)(bytesReceived * 100 / totalBytes.Value)
                : -1;

            if (percent == lastReportedPercent)
                continue;

            progress?.Report(new DownloadProgressReport(filename, percent));
            lastReportedPercent = percent;
        }
    }

    private async Task<Result> DownloadFileAsync(string url, string destinationDirectory, string filename,
        IProgress<DownloadProgressReport>? progress)
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
        var fileLock = s_downloadLocks.GetOrAdd(destinationPath, _ =>
            new SemaphoreSlim(1, 1));

        await fileLock.WaitAsync();

        try
        {
            if (File.Exists(destinationPath))
            {
                progress?.Report(new DownloadProgressReport(filename, 100));
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
        IProgress<DownloadProgressReport>? progress)
    {
        try
        {
            LogFileDownloadStart(filename, destinationDirectory, url);

            var httpClient = _clientFactory.CreateClient(HttpClientName);
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
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
                    progress?.Report(new DownloadProgressReport(filename, percent));
                    lastReportedPercent = percent;
                }
            }

            LogFileDownloadSuccess(filename, destinationDirectory);

            return Result.Success();
        }
        // TODO: handle mod download failures with cleanups
        catch (TaskCanceledException ex)
        {
            progress?.Report(new DownloadProgressReport(filename, -1));
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
            return Result.Error($"Failed to download {filename} from {url}", ErrorType.None, ex);
        }
    }

    private void TryDeleteTempFile(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
        catch (IOException ex)
        {
            LogTempFileCleanupFailure(tempPath, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogTempFileCleanupFailure(tempPath, ex);
        }
    }
}
