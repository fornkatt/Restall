using Restall.Application.DTOs;
using Restall.Application.Interfaces;
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

        if (!await DownloadAsync(request, addonFileName, progress))
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

    private Task<bool> DownloadAsync(InstallRenoDXRequest request, string addonFileName, IProgress<DownloadProgressReportDto>? progress = null)
    {
        var isUnity = request.GenericModInfo?.Engine == Engine.Unity ||
            (request.GenericModInfo is null && request.Game.EngineName == Game.Engine.Unity);

        return isUnity
            ? _modDownloadService.DownloadUnityRenoDXAsync(addonFileName, progress)
            : _modDownloadService.DownloadRenoDXAsync(
                request.Branch,
                addonFileName,
                wikiSnapshotUrl: request.Arch == RenoDX.Architecture.x64 ? request.ModInfo?.SnapshotUrl64 : request.ModInfo?.SnapshotUrl32,
                progress: progress
                );
    }
}