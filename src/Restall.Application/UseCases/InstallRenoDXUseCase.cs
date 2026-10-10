// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.DTOs.Requests;
using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.Logging;
using Restall.Application.Services;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

// TODO: might be able to lean more into Result overall in this file
public sealed partial class InstallRenoDXUseCase : IInstallRenoDXUseCase
{
    private readonly ILogger<InstallRenoDXUseCase> _logger;
    private readonly IDownloadService _downloadService;
    private readonly IModInstallService _modInstallService;
    private readonly IModDetectionService _modDetectionService;
    private readonly IFileService _fileService;
    private readonly IPathService _pathService;
    private readonly GameEntryAssembler _gameEntryAssembler;

    public InstallRenoDXUseCase(
        ILogger<InstallRenoDXUseCase> logger,
        IDownloadService downloadService,
        IModInstallService modInstallService,
        IModDetectionService modDetectionService,
        IFileService fileService,
        IPathService pathService,
        GameEntryAssembler gameEntryAssembler
    )
    {
        _logger = logger;
        _downloadService = downloadService;
        _modInstallService = modInstallService;
        _modDetectionService = modDetectionService;
        _fileService = fileService;
        _pathService = pathService;
        _gameEntryAssembler = gameEntryAssembler;
    }

    public async Task<RenoDXInstallResponse> ExecuteAsync(RenoDXInstallRequest request,
        IProgress<DownloadProgressReport>? progress = null)
    {
        var game = request.Game;
        var gameName = game.Name ?? "Unknown";
        var gameEntry = _gameEntryAssembler.Assemble(game);

        if (gameEntry.RenoDXEntry.DownloadOptions is not { } downloadOptions)
        {
            LogRenoDXDownloadNotFound(gameName, request.Branch);
            return new RenoDXInstallResponse(false, game, null,
                GetNoDownloadMessage(gameEntry.RenoDXEntry.ManualSource));
        }

        if (GetDownloadSource(downloadOptions, request) is not { } downloadSource)
        {
            LogRenoDXDownloadNotFound(gameName, request.Branch);
            return new RenoDXInstallResponse(false, game, null,
                GetReleaseUnavailableMessage(request));
        }

        var filename = downloadOptions.Filename;

        _logger.ModInstallationStart("RenoDX", filename,
            gameEntry.RecommendedArchitecture.ToString(), gameName,
            game.ExecutablePath ?? "Unknown");

        var isCached = downloadSource.CanReuseCachedFile && _fileService.FileExists(downloadSource.DestinationPath);

        if (!isCached)
        {
            var downloadResult = await _downloadService.DownloadAsync(downloadSource.Url,
                downloadSource.DestinationPath, progress);

            if (!downloadResult.IsSuccess)
            {
                _logger.ModDownloadFailure("RenoDX", downloadSource.DestinationPath,
                    downloadResult.Message, downloadResult.Exception);

                return new RenoDXInstallResponse(false, game, null,
                    GetFailureMessage(downloadResult.ErrorType, "download", filename));
            }
        }

        var renoDXVersion = _modDetectionService.GetRenoDXFileVersion(downloadSource.DestinationPath);

        if (!renoDXVersion.IsSuccess)
            LogRenoDXVersionReadFailure(filename, gameName, renoDXVersion.Message,
                renoDXVersion.Exception);

        var renoDX = new RenoDX
        {
            SelectedName = downloadOptions.IsInstalledFile ? game.RenoDX?.SelectedName ?? filename : filename,
            OriginalName = filename,
            BranchName = request.Branch,
            Arch = gameEntry.RecommendedArchitecture,
            Version = renoDXVersion.Value
        };

        if (game.RenoDX is { } installedRenoDX)
        {
            var installedFilename = installedRenoDX.SelectedName ?? filename;
            var deleteResult = _fileService.TryDeleteFile(Path.Combine(game.ExecutablePath!,
                installedFilename), RenoDX.OriginalFilenameStart);
            var wasAlreadyRemoved = deleteResult.ErrorType is ErrorType.FileNotFound;

            if (!deleteResult.IsSuccess && !wasAlreadyRemoved)
            {
                _logger.ExistingModFileDeletionFailure("RenoDX", gameName,
                    deleteResult.Message, deleteResult.Exception);

                return new RenoDXInstallResponse(false, game, null,
                    GetFailureMessage(deleteResult.ErrorType, "remove", installedFilename));
            }
        }

        var installResult = await _modInstallService.InstallModAsync(game, renoDX, downloadSource.DestinationPath);

        if (!installResult.IsSuccess)
        {
            _logger.ModInstallationFailure("RenoDX", gameName, installResult.Message,
                installResult.Exception);

            return new RenoDXInstallResponse(false, game, null,
                GetFailureMessage(installResult.ErrorType, "install", filename));
        }

        _logger.ModInstallationComplete("RenoDX", filename, renoDX.Arch.ToString(), gameName);

        return new RenoDXInstallResponse(true, game, _gameEntryAssembler.Assemble(game),
            GetSuccessMessage(filename, renoDX.Version));
    }

    private DownloadSource? GetDownloadSource(RenoDXDownloadOptions downloadOptions, RenoDXInstallRequest request) =>
        request.Branch switch
        {
            RenoDX.Branch.Snapshot when downloadOptions.Snapshot is { } snapshot => new DownloadSource(
                snapshot.GetDownloadUrl(downloadOptions.Filename),
                _pathService.GetRenoDXSnapshotDownloadPath(
                    snapshot.Version, downloadOptions.Filename),
                true),
            RenoDX.Branch.Nightly when GetRequestedNightly(downloadOptions, request.NightlyVersion) is { } nightly =>
                new DownloadSource(
                    nightly.GetDownloadUrl(downloadOptions.Filename),
                    _pathService.GetRenoDXNightlyDownloadPath(
                        nightly.Version, downloadOptions.Filename),
                    true),
            RenoDX.Branch.Direct when downloadOptions.DirectUrl is { } directUrl => new DownloadSource(
                directUrl,
                _pathService.GetRenoDXDirectDownloadPath(downloadOptions.Filename),
                false),
            _ => null
        };

    private static RenoDXTagInfo? GetRequestedNightly(RenoDXDownloadOptions downloadOptions, string? nightlyVersion) =>
        nightlyVersion is null ? downloadOptions.LatestNightly : downloadOptions.GetNightly(nightlyVersion);

    private static string GetNoDownloadMessage(RenoDXModLink? manualSource) => manualSource switch
    {
        { } link => $"Restall is not able to download this game's RenoDX mod. Get it from {link.Label} instead.",
        null => "There's no RenoDX download for this game."
    };

    private static string GetReleaseUnavailableMessage(RenoDXInstallRequest request) => request.Branch switch
    {
        RenoDX.Branch.Nightly when request.NightlyVersion is { } nightlyVersion =>
            $"Nightly {nightlyVersion} doesn't include this game's RenoDX mod file. " +
            $"Pick another Nightly and try again.",
        _ => $"This game's RenoDX mod file is not available for branch {request.Branch}."
    };

    private static string GetFailureMessage(ErrorType errorType, string action, string filename) =>
        $"Couldn't {action} {filename}. " + errorType switch
        {
            ErrorType.PermissionDenied => "Restall doesn't have permission to change files in that location. " +
                                          "Check your permissions and try again.",
            ErrorType.FileSystemError => "The disk may be full or the file may be in use (is the game running?).",
            ErrorType.NetworkTimeout => "The connection timed out. Check your internet connection and try again.",
            ErrorType.DownloadFailed => "The server may be unavailable or the file may no longer exist.",
            _ => "Check the log for details."
        };

    private static string GetSuccessMessage(string filename, string? version) => version switch
    {
        null =>
            $"Successfully installed {filename} but version could not be read so it might not appear in the UI.\n\n" +
            $"Check the logs for details.",
        _ => $"Successfully installed {filename} with version {version}."
    };

    private sealed record DownloadSource(Uri Url, string DestinationPath, bool CanReuseCachedFile);
}
