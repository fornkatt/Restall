using Restall.Application.Common;
using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public sealed class InstallReShadeUseCase : IInstallReShadeUseCase
{
    private readonly IPathService _pathService;
    private readonly IModDownloadService _modDownloadService;
    private readonly IFileExtractionService _fileExtractionService;
    private readonly IModInstallService _modInstallService;
    private readonly ILogService _logService;

    public InstallReShadeUseCase(
        IPathService pathService,
        IModDownloadService modDownloadService,
        IFileExtractionService fileExtractionService,
        IModInstallService modInstallService,
        ILogService logService
    )
    {
        _pathService = pathService;
        _modDownloadService = modDownloadService;
        _fileExtractionService = fileExtractionService;
        _modInstallService = modInstallService;
        _logService = logService;
    }

    public async Task<ModOperationResultDto> ExecuteAsync(InstallReShadeRequest request,
        IProgress<DownloadProgressReportDto>? progress = null)
    {
        var reShade = new ReShade
        {
            BranchName = request.Branch,
            Arch = request.Arch,
            Version = request.Version,
            SelectedFilename = request.SelectedFilename,
        };

        var extractedFilePath = _pathService.GetReShadeExtractedFilePath(reShade);

        if (!File.Exists(extractedFilePath))
        {
            var downloaded = await EnsureDownloadedAsync(reShade, progress);

            if (!downloaded.IsSuccess)
            {
                await _logService.LogErrorAsync(downloaded.ErrorMessage ?? "Failed to download ReShade installer.", downloaded.Exception);

                var userMessage = downloaded.Error switch
                {
                    ResultError.PermissionDenied => "Permission denied downloading the ReShade installer to cache. " +
                                                    "Please ensure your have read/write access to the appropriate directories and try again.",
                    ResultError.FileSystemError => "Something went wrong writing cache directories or downloading files to cache. " +
                                                   "Please ensure the destination is not in use and the disk is not full and try again.",
                    ResultError.DownloadFailed => "Could not download ReShade installer. The server may be unavailable or  the file may no longer exist.",
                    ResultError.NetworkTimeout => "Connection timed out while downloading ReShade installer. Please check your internet connection and try again.",
                    _ => "Failed to download ReShade installer. Check logs for details."
                };
                
                return new ModOperationResultDto(false, request.Game, userMessage);
            }

            var installerPath = _pathService.GetReShadeInstallerFilePath(reShade.BranchName, reShade.Version);
            var extractedCacheDir = _pathService.GetReShadeCachePath(reShade);

            var extractionResult =
                _fileExtractionService.ExtractFiles(installerPath, [reShade.OriginalFileName], extractedCacheDir);

            if (!extractionResult.IsSuccess)
            {
                await _logService.LogErrorAsync(extractionResult.ErrorMessage ?? "Failed to extract files from installer", extractionResult.Exception);

                var userMessage = extractionResult.Error switch
                {
                    ResultError.ToolNotFound => OperatingSystem.IsLinux() 
                    ? "bsdtar not found. Please install libarchive-tools and try again."
                    : "tar not found. Enure it is available on your system.",
                    ResultError.PermissionDenied => "Permission denied extracting the ReShade installer files to cache. " +
                                                    "Please ensure your have read/write access to the appropriate directories and try again.",
                    ResultError.FileSystemError => "Something went wrong writing cache directories or files to cache. " +
                                                   "Please ensure the destination is not in use and the disk is not full and try again.",
                    ResultError.ProcessStartFailed => "Extraction process failed to start. " +
                                                      "The binary may be corrupt or missing execute permissions. Check logs for more details.",
                    ResultError.ExtractionFailed => "File extraction failed. " +
                                                    "Please ensure the files are not in use and there's enough disk space available and try again.",
                    _ => "An unexpected error occured during file extraction. Check logs for more details."
                };
                
                return new ModOperationResultDto(false, request.Game, userMessage);
            }
        }

        var result = await _modInstallService.InstallModAsync(request.Game, reShade, extractedFilePath);

        if (!result.IsSuccess)
        {
            await _logService.LogErrorAsync(result.ErrorMessage ?? "Failed to install ReShade.", result.Exception);

            var userMessage = result.Error switch
            {
                ResultError.PermissionDenied => "Permission denied installing ReShade to the game directory. " +
                                                "Please ensure your have read/write access to the appropriate directories and try again.",
                ResultError.FileSystemError => "Something went wrong writing ReShade files to the game folder. " +
                                               "Please ensure the destination is not in use and the disk is not full and try again.",
                _ => "Failed to install ReShade. Check logs for details."
            };
            
            return new ModOperationResultDto(false, request.Game, userMessage);
        }

        return new ModOperationResultDto(true, result.Value!, $"Successfully installed ReShade as {reShade.SelectedFilename}!");
    }

    private async Task<Result> EnsureDownloadedAsync(ReShade reShade, IProgress<DownloadProgressReportDto>? progress)
    {
        var installerPath = _pathService.GetReShadeInstallerFilePath(reShade.BranchName, reShade.Version!);

        if (File.Exists(installerPath))
            return Result.Ok();

        return await _modDownloadService.DownloadReShadeAsync(reShade.BranchName, reShade.Version!, progress);
    }
}