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

public sealed class UninstallReShadeUseCase : IUninstallReShadeUseCase
{
    private readonly ILogger<UninstallReShadeUseCase> _logger;
    private readonly IModInstallService _modInstallService;
    private readonly GameEntryAssembler _gameEntryAssembler;

    public UninstallReShadeUseCase(
        ILogger<UninstallReShadeUseCase> logger,
        IModInstallService modInstallService,
        GameEntryAssembler gameEntryAssembler
    )
    {
        _logger = logger;
        _modInstallService = modInstallService;
        _gameEntryAssembler = gameEntryAssembler;
    }

    public ModOperationResponse Execute(Game game)
    {
        _logger.ModUninstallationStart("ReShade", game.Name ?? "Unknown", game.ExecutablePath ?? "Unknown");

        var result = _modInstallService.UninstallReShade(game);

        if (!result.IsSuccess)
        {
            var gameName = game.Name ?? "Unknown";

            var userMessage = result.ErrorType switch
            {
                ErrorType.PermissionDenied =>
                    $"Permission denied uninstalling ReShade from {gameName}. " +
                    $"Check your app permissions and try again.",
                ErrorType.FileSystemError =>
                    $"Failed to uninstall ReShade from {gameName}. " +
                    $"The disk may be full or the file may be locked (game running?).",
                ErrorType.FileNotFound =>
                    "File not found at expected location. It might have been moved or deleted. " +
                    "Please perform a full rescan.",
                _ => $"Failed to uninstall ReShade from {gameName}. Check the log for details."
            };

            _logger.ModUninstallFailure("ReShade", gameName, result.Message, result.Exception);

            return new ModOperationResponse(false, game, userMessage);
        }

        _logger.ModUninstallationComplete("ReShade", game.Name ?? "Unknown");

        return new ModOperationResponse(true, result.Value!, "Successfully uninstalled ReShade!",
            GameEntry: _gameEntryAssembler.Assemble(result.Value!));
    }
}
