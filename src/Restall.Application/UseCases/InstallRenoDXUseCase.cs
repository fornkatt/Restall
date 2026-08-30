using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.Logging;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

// TODO: might be able to lean more into Result overall in this file
public sealed partial class InstallRenoDXUseCase : IInstallRenoDXUseCase
{
    private readonly ILogger<InstallRenoDXUseCase> _logger;
    private readonly IModDownloadService _modDownloadService;
    private readonly IModInstallService _modInstallService;
    private readonly IModDetectionService _modDetectionService;
    private readonly IFileService _fileService;
    private readonly IPathService _pathService;

    public InstallRenoDXUseCase(
        ILogger<InstallRenoDXUseCase> logger,
        IModDownloadService modDownloadService,
        IModInstallService modInstallService,
        IModDetectionService modDetectionService,
        IFileService fileService,
        IPathService pathService
    )
    {
        _logger = logger;
        _modDownloadService = modDownloadService;
        _modInstallService = modInstallService;
        _modDetectionService = modDetectionService;
        _fileService = fileService;
        _pathService = pathService;
    }

    public async Task<ModOperationResultDto> ExecuteAsync(InstallRenoDXRequest request,
        IProgress<DownloadProgressReportDto>? progress = null)
    {
        var addonFilename = ResolveAddonFilename(request);

        if (addonFilename is null)
        {
            LogRenoDXAddonFilenameResolutionFailure(request.Game.Name ?? "Unknown", request.Game.EngineName.ToString(),
                request.Arch.ToString());

            return new ModOperationResultDto(
                false,
                request.Game,
                """
                Could not determine addon filename.


                This game has no wiki entry or is Discord/Nexus only and no existing RenoDX installation was detected.
                """
            );
        }

        var isUnityGeneric = request.GenericModInfo?.Engine == SupportedEngine.Unity ||
                             (request.GenericModInfo is null &&
                              request.ModInfo is not { HasWikiFilename: true } &&
                              request.Game.EngineName == Game.Engine.Unity);

        var renoDX = new RenoDX
        {
            SelectedName = request.Game.RenoDX is not null ? request.Game.RenoDX.SelectedName : addonFilename,
            OriginalName = addonFilename,
            BranchName = isUnityGeneric ? RenoDX.Branch.Wiki : request.Branch,
            Arch = request.Arch
        };

        await InvalidateCacheIfOutdatedAsync(renoDX, request.TargetVersion, isUnityGeneric);

        var downloadResult = await DownloadAsync(isUnityGeneric, request, addonFilename, progress);

        if (!downloadResult.IsSuccess)
        {
            var userMessage = downloadResult.ErrorType switch
            {
                ErrorType.PermissionDenied =>
                    $"Permission denied writing {addonFilename} to the cache folder. Check your app permissions and try again.",
                ErrorType.FileSystemError =>
                    $"Failed to write {addonFilename} to disk. The disk may be full or the file may be locked.",
                ErrorType.NetworkTimeout =>
                    $"Connection timed out while downloading {addonFilename}. Please check your internet connection and try again.",
                ErrorType.DownloadFailed =>
                    $"Download failed for {addonFilename}. The server may be unavailable or the file may no longer exist.",
                _ => $"Failed to download {addonFilename}. Check log for details."
            };

            _logger.ModDownloadFailure("RenoDX", _pathService.GetRenoDXDownloadCacheDirectory(renoDX.BranchName),
                downloadResult.ErrorMessage, downloadResult.Exception);

            return new ModOperationResultDto(false, request.Game, userMessage);
        }

        var filePath = _pathService.GetRenoDXCachePath(renoDX);

        var renoDxVersion = _modDetectionService.GetRenoDXFileVersion(filePath);

        if (!renoDxVersion.IsSuccess)
            LogRenoDXVersionReadFailure(addonFilename, request.Game.Name ?? "Unknown", renoDxVersion.ErrorMessage,
                renoDxVersion.Exception);

        renoDX.Version = renoDxVersion.Value;

        if (request.Game.RenoDX is { } existingRenoDX)
        {
            var deleteResult =
                _fileService.TryDeleteFile(Path.Combine(request.Game.ExecutablePath!,
                    existingRenoDX.SelectedName ?? addonFilename));

            if (!deleteResult.IsSuccess)
            {
                var userMessage = deleteResult.ErrorType switch
                {
                    ErrorType.PermissionDenied =>
                        "Permission denied deleting existing RenoDX file. Please ensure you have write access to the game directory and try again.",
                    ErrorType.FileSystemError =>
                        "Something went wrong uninstalling an existing RenoDX file. Please ensure the file is not in use and the disk is not full and try again.",
                    ErrorType.FileNotFound =>
                        "File not found at expected location. It might have been moved or deleted. Please perform a full rescan.",
                    _ => "Unexpected error occurred while uninstalling existing mod record. Check logs for details."
                };

                _logger.ExistingModFileDeletionFailure("RenoDX", request.Game.Name ?? "Unknown",
                    deleteResult.ErrorMessage, deleteResult.Exception);

                return new ModOperationResultDto(false, request.Game, userMessage);
            }
        }

        var installResult = await _modInstallService.InstallModAsync(request.Game, renoDX, filePath);

        if (!installResult.IsSuccess)
        {
            var userMessage = installResult.ErrorType switch
            {
                ErrorType.PermissionDenied =>
                    $"Permission denied writing {addonFilename} to the game folder. Check your app permissions and try again.",
                ErrorType.FileSystemError =>
                    $"Failed to write {addonFilename} to disk. The disk may be full or the file may be locked.",
                _ => $"Failed to install {addonFilename}. Check log for details."
            };

            _logger.ModInstallationFailure("RenoDX", request.Game.Name ?? "Unknown", installResult.ErrorMessage,
                installResult.Exception);

            return new ModOperationResultDto(false, request.Game, userMessage);
        }

        var versionNote = renoDX.Version is not null
            ? ""
            : $"\n\nVersion could not be read from {addonFilename}.It may not appear in the UI.\n" +
              $"Check the logs for information on why this might have happened.";
        return new ModOperationResultDto(true, request.Game, $"Successfully installed {addonFilename}!{versionNote}");
    }

    private async Task InvalidateCacheIfOutdatedAsync(RenoDX renoDX, string? targetVersion,
        bool forceInvalidate = false)
    {
        var cachedFilePath = _pathService.GetRenoDXCachePath(renoDX);
        if (!File.Exists(cachedFilePath)) return;

        if (!forceInvalidate)
        {
            if (string.IsNullOrWhiteSpace(targetVersion)) return;

            if (!DateOnly.TryParseExact(targetVersion, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None,
                    out _)) return;

            var cachedVersion = _modDetectionService.GetRenoDXFileVersion(cachedFilePath);
            if (!IsCacheOutdated(cachedVersion.Value, targetVersion)) return;
        }

        var deleted = _fileService.TryDeleteFile(cachedFilePath);

        // TODO: develop proper failure path? A stale file might give the user a version different from what was requested
        if (!deleted.IsSuccess)
            LogRenoDXStaleCacheDeletionFailure(cachedFilePath, deleted.ErrorMessage, deleted.Exception);
    }

    private static bool IsCacheOutdated(string? cachedVersion, string? targetVersion)
    {
        if (string.IsNullOrWhiteSpace(cachedVersion) || string.IsNullOrWhiteSpace(targetVersion))
            return false;

        return DateOnly.TryParseExact(cachedVersion, "yyyyMMdd", null,
                   System.Globalization.DateTimeStyles.None, out var cached) &&
               DateOnly.TryParseExact(targetVersion, "yyyyMMdd", null,
                   System.Globalization.DateTimeStyles.None, out var target) &&
               target != cached;
    }

    private string? ResolveAddonFilename(InstallRenoDXRequest request)
    {
        if (request.Game.RenoDX?.OriginalName is { } originalName)
            return originalName;

        if (request.ModInfo is { HasWikiFilename: true } modInfo)
            return request.Arch == RenoDX.Architecture.x32
                ? modInfo.AddonFilename32 ?? modInfo.AddonFilename64
                : modInfo.AddonFilename64 ?? modInfo.AddonFilename32;

        if (request.GenericModInfo is { } generic)
            return request.Arch == RenoDX.Architecture.x64 ? generic.AddonFilename64 : generic.AddonFilename32;

        var bit = request.Arch == RenoDX.Architecture.x64 ? "64" : "32";

        var engineBased = request.Game.EngineName switch
        {
            Game.Engine.Unity => $"renodx-unityengine.addon{bit}",
            Game.Engine.Unreal => $"renodx-unrealengine.addon{bit}",
            _ => null
        };
        if (engineBased is not null)
            return engineBased;

        return null;
    }

    private Task<Result> DownloadAsync(bool isUnityEngine, InstallRenoDXRequest request, string addonFilename,
        IProgress<DownloadProgressReportDto>? progress = null)
    {
        return isUnityEngine
            ? _modDownloadService.DownloadUnityRenoDXAsync(addonFilename, progress)
            : _modDownloadService.DownloadRenoDXAsync(
                request.Branch,
                addonFilename,
                version: request.TargetVersion,
                wikiSnapshotUrl: request.Arch == RenoDX.Architecture.x64
                    ? request.ModInfo?.SnapshotUrl64
                    : request.ModInfo?.SnapshotUrl32,
                progress: progress
            );
    }
}