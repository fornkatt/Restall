using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public sealed class InstallRenoDXUseCase : IInstallRenoDXUseCase
{
    private readonly IModDownloadService _modDownloadService;
    private readonly IModInstallService _modInstallService;
    private readonly IModDetectionService _modDetectionService;
    private readonly ICachePathService _cachePathService;
    private readonly ILogService _logService;

    public InstallRenoDXUseCase(
        IModDownloadService modDownloadService,
        IModInstallService modInstallService,
        IModDetectionService modDetectionService,
        ICachePathService cachePathService,
        ILogService logService
        )
    {
        _modDownloadService = modDownloadService;
        _modInstallService = modInstallService;
        _modDetectionService = modDetectionService;
        _cachePathService = cachePathService;
        _logService = logService;
    }

    public async Task<ModOperationResultDto> ExecuteAsync(InstallRenoDXRequest request, IProgress<DownloadProgressReportDto>? progress = null)
    {
        var addonFilename = ResolveAddonFilename(request);

        if (addonFilename is null)
            return new ModOperationResultDto(
                false,
                request.Game,
                "Could not determine addon filename.\n\n" +
                "This game has no wiki entry or is Discord/Nexus only and no existing RenoDX installation was detected."
                );

        var renoDx = new RenoDX
        {
            SelectedName = addonFilename,
            OriginalName = addonFilename,
            BranchName = request.Branch,
            Arch = request.Arch
        };

        var isUnityEngine = request.GenericModInfo?.Engine == Engine.Unity ||
            (request.GenericModInfo is null && request.Game.EngineName == Game.Engine.Unity);

        await InvalidateCacheIfOutdatedAsync(renoDx, request.TargetVersion, isUnityEngine);

        if (!await DownloadAsync(isUnityEngine, request, addonFilename, progress))
        {
            await _logService.LogWarningAsync($"Failed to download RenoDX: {addonFilename}");
            return new ModOperationResultDto(false, request.Game, "Failed to download file.");
        }

        var filePath = _cachePathService.GetRenoDXCachePath(renoDx);
        var renoDxVersion = _modDetectionService.GetRenoDXFileVersion(filePath);
        renoDx.Version = renoDxVersion;

        var result = await _modInstallService.InstallModAsync(request.Game, renoDx, filePath);

        if (result.IsSuccess)
            await _logService.LogInfoAsync($"Successfully installed RenoDX as {renoDx.SelectedName} to game: {request.Game.Name}");
        else
            await _logService.LogWarningAsync($"Could not install RenoDX to game: {request.Game.Name}");

        return result;
    }

    private async Task InvalidateCacheIfOutdatedAsync(RenoDX renoDx, string? targetVersion, bool forceInvalidate = false)
    {
        var cachedFilePath = _cachePathService.GetRenoDXCachePath(renoDx);
        if (!File.Exists(cachedFilePath)) return;

        if (string.IsNullOrWhiteSpace(targetVersion)) return;

        var cachedVersion = _modDetectionService.GetRenoDXFileVersion(cachedFilePath);
        if (!IsCacheOutdated(cachedVersion, targetVersion)) return;

        try
        {
            File.Delete(cachedFilePath);
            await _logService.LogInfoAsync($"Stale cache invalidated for {renoDx.OriginalName}");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Could not delete stale cache file: {cachedFilePath}", ex);
        }
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

    private Task<bool> DownloadAsync(bool isUnityEngine, InstallRenoDXRequest request, string addonFilename, IProgress<DownloadProgressReportDto>? progress = null)
    {
        return isUnityEngine
            ? _modDownloadService.DownloadUnityRenoDXAsync(addonFilename, progress)
            : _modDownloadService.DownloadRenoDXAsync(
                request.Branch,
                addonFilename,
                version: request.TargetVersion,
                wikiSnapshotUrl: request.Arch == RenoDX.Architecture.x64 ? request.ModInfo?.SnapshotUrl64 : request.ModInfo?.SnapshotUrl32,
                progress: progress
                );
    }
}