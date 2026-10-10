// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.Logging;
using Restall.Application.Services;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public sealed class UninstallRenoDXUseCase : IUninstallRenoDXUseCase
{
    private readonly ILogger<UninstallRenoDXUseCase> _logger;
    private readonly IFileService _fileService;
    private readonly GameEntryAssembler _gameEntryAssembler;

    public UninstallRenoDXUseCase(
        ILogger<UninstallRenoDXUseCase> logger,
        IFileService fileService,
        GameEntryAssembler gameEntryAssembler
    )
    {
        _logger = logger;
        _fileService = fileService;
        _gameEntryAssembler = gameEntryAssembler;
    }

    public RenoDXUninstallResponse Execute(Game game)
    {
        var gameName = game.Name ?? "Unknown";

        _logger.ModUninstallationStart("RenoDX", gameName,
            game.ExecutablePath ?? "Unknown");

        var path = Path.Combine(game.ExecutablePath!, game.RenoDX!.SelectedName!);

        var deleteResult = _fileService.TryDeleteFile(path, RenoDX.OriginalFilenameStart);

        var wanAlreadyRemoved = deleteResult.ErrorType is ErrorType.FileNotFound;

        if (!deleteResult.IsSuccess && !wanAlreadyRemoved)
        {
            _logger.ModUninstallFailure("RenoDX", gameName, deleteResult.Message, deleteResult.Exception);

            return new RenoDXUninstallResponse(false, game, _gameEntryAssembler.Assemble(game),
                GetFailureMessage(deleteResult.ErrorType, gameName));
        }

        game.RenoDX = null;

        _logger.ModUninstallationComplete("RenoDX", gameName);

        return new RenoDXUninstallResponse(true, game, _gameEntryAssembler.Assemble(game),
            GetSuccessMessage(wanAlreadyRemoved));
    }

    private static string GetFailureMessage(ErrorType errorType, string gameName) => errorType switch
    {
        ErrorType.PermissionDenied =>
            $"Permission denied uninstalling RenoDX from {gameName}. Check your app permissions and try again.",
        ErrorType.FileSystemError => $"Failed to uninstall RenoDX from {gameName}." +
                                     $" The disk may be full or the file may be locked (is the game running?).",
        _ => $"Failed to uninstall RenoDX from {gameName}. Check the logs for details."
    };

    private static string GetSuccessMessage(bool wasAlreadyRemvoed) => wasAlreadyRemvoed switch
    {
        true => "RenoDX could not be found at the expected location so we cleared it from the game.\n" +
                "If you believe this was an error, please perform a rescan.",
        false => "Successfully uninstalled RenoDX!"
    };
}
