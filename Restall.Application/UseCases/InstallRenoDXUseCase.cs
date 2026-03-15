using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public class InstallRenoDXUseCase : IInstallRenoDXUseCase
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
        var addonFileName = ResolveAddonFileName(request);

        if (addonFileName is null)
            return new ModOperationResultDto(false, request.Game, "No mod info provided.");

        var renoDx = new RenoDX
        {
            SelectedName = addonFileName,
            OriginalName = addonFileName,
            BranchName = request.Branch,
            Arch = request.Arch,
            Maintainer = request.ModInfo?.Maintainer
        };

        var isUnityEngine = request.GenericModInfo?.Engine == Engine.Unity ||
            (request.GenericModInfo is null && request.Game.EngineName == Game.Engine.Unity);

        await InvalidateCacheIfOutdatedAsync(renoDx, request.TargetVersion, isUnityEngine);

        if (!await DownloadAsync(isUnityEngine, request, addonFileName, progress))
        {
            await _logService.LogWarningAsync($"Failed to download RenoDX: {addonFileName}");
            return new ModOperationResultDto(false, request.Game, "Failed to download RenoDX");
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

        string? reason;

        if (forceInvalidate)
        {
            reason = "always fetch fresh version for external mods";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(targetVersion)) return;

            var cachedVersion = _modDetectionService.GetRenoDXFileVersion(cachedFilePath);
            if (!IsCacheOutdated(cachedVersion, targetVersion)) return;

            reason = $"cached = {cachedVersion}, target = {targetVersion}";
        }

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

        const string nightlyPrefix = "nightly-";

        var targetDate = targetVersion.StartsWith(nightlyPrefix, StringComparison.OrdinalIgnoreCase)
            ? targetVersion[nightlyPrefix.Length..]
            : targetVersion;

        return DateOnly.TryParseExact(cachedVersion, "yyyyMMdd", null,
            System.Globalization.DateTimeStyles.None, out var cached) &&
               DateOnly.TryParseExact(targetDate, "yyyyMMdd", null,
            System.Globalization.DateTimeStyles.None, out var target) &&
            target != cached;
    }

    private string? ResolveAddonFileName(InstallRenoDXRequest request)
    {
        if (request.ModInfo is { } modInfo)
            return request.Arch == RenoDX.Architecture.x32
                ? modInfo.AddonFileName32 ?? modInfo.AddonFileName64
                : modInfo.AddonFileName64 ?? modInfo.AddonFileName32;

        if (request.GenericModInfo is { } generic)
            return request.Arch == RenoDX.Architecture.x64 ? generic.AddonFileName64 : generic.AddonFileName32;

        var bit = request.Arch == RenoDX.Architecture.x64 ? "64" : "32";

        return request.Game.EngineName switch
        {
            Game.Engine.Unity => $"renodx-unityengine.addon{bit}",
            Game.Engine.Unreal => $"renodx-unrealengine.addon{bit}",
            _ => null
        };
    }

    private Task<bool> DownloadAsync(bool isUnityEngine, InstallRenoDXRequest request, string addonFileName, IProgress<DownloadProgressReportDto>? progress = null)
    {
        return isUnityEngine
            ? _modDownloadService.DownloadUnityRenoDXAsync(addonFileName, progress)
            : _modDownloadService.DownloadRenoDXAsync(
                request.Branch,
                addonFileName,
                version: request.TargetVersion,
                wikiSnapshotUrl: request.Arch == RenoDX.Architecture.x64 ? request.ModInfo?.SnapshotUrl64 : request.ModInfo?.SnapshotUrl32,
                progress: progress
                );
    }
}