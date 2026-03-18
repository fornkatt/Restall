using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public class InstallReShadeUseCase : IInstallReShadeUseCase
{
    private readonly ICachePathService _cachePathService;
    private readonly IModDownloadService _modDownloadService;
    private readonly IFileExtractionService _fileExtractionService;
    private readonly IModInstallService _modInstallService;
    private readonly ILogService _logService;

    public InstallReShadeUseCase(
        ICachePathService cachePathService,
        IModDownloadService modDownloadService,
        IFileExtractionService fileExtractionService,
        IModInstallService modInstallService,
        ILogService logService
        )
    {
        _cachePathService = cachePathService;
        _modDownloadService = modDownloadService;
        _fileExtractionService = fileExtractionService;
        _modInstallService = modInstallService;
        _logService = logService;
    }

    public async Task<ModOperationResultDto> ExecuteAsync(InstallReShadeRequest request, IProgress<DownloadProgressReportDto>? progress = null)
    {
        var reShade = new ReShade
        {
            BranchName = request.Branch,
            Arch = request.Arch,
            Version = request.Version,
            SelectedFilename = request.SelectedFilename,
        };

        var extractedFilePath = _cachePathService.GetReShadeExtractedFilePath(reShade);

        if (!File.Exists(extractedFilePath))
        {
            var downloaded = await EnsureDownloadedAsync(reShade, progress);

            if (!downloaded)
                return new ModOperationResultDto(false, request.Game, "Failed to download ReShade installer.");

            var extracted = ExtractFromDownload(reShade);
            if (!extracted)
                return new ModOperationResultDto(false, request.Game, "Failed to extract files from installer.");
        }

        var result = await _modInstallService.InstallModAsync(request.Game, reShade, extractedFilePath);

        if (result.IsSuccess)
            await _logService.LogInfoAsync($"Successfully installed ReShade version {reShade.Version} as {reShade.SelectedFilename} to game: {request.Game.Name}");
        else
            await _logService.LogWarningAsync($"Could not install ReShade to game: {request.Game.Name}");

        return result;
    }

    private async Task<bool> EnsureDownloadedAsync(ReShade reShade, IProgress<DownloadProgressReportDto>? progress)
    {
        var installerPath = _cachePathService.GetReShadeInstallerFilePath(reShade.BranchName, reShade.Version!);

        if (File.Exists(installerPath))
        {
            await _logService.LogInfoAsync($"ReShade {reShade.Version} installer already in download cache, skipping download.");
            return true;
        }

        return await _modDownloadService.DownloadReShadeAsync(reShade.BranchName, reShade.Version!, progress);
    }

    private bool ExtractFromDownload(ReShade reShade)
    {
        var installerPath = _cachePathService.GetReShadeInstallerFilePath(reShade.BranchName, reShade.Version!);
        var extractedCacheDir = _cachePathService.GetReShadeCachePath(reShade);

        return _fileExtractionService.ExtractFiles(installerPath, [reShade.OriginalFileName], extractedCacheDir);
    }
}