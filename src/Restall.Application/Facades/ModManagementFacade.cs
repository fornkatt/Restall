// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.DTOs;
using Restall.Application.DTOs.Requests;
using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.Facades;

// TODO: combine with UseCases, add global exception handler instead
public sealed partial class ModManagementFacade : IModManagementFacade
{
    private readonly ILogger<ModManagementFacade> _logger;
    private readonly IInstallReShadeUseCase _installReShadeUseCase;
    private readonly IUninstallReShadeUseCase _uninstallReShadeUseCase;
    private readonly IInstallRenoDXUseCase _installRenoDXUseCase;
    private readonly IUninstallRenoDXUseCase _uninstallRenoDXUseCase;
    private readonly IUpdateCheckService _updateCheckService;

    public ModManagementFacade(
        ILogger<ModManagementFacade> logger,
        IInstallReShadeUseCase installReShadeUseCase,
        IUninstallReShadeUseCase uninstallReShadeUseCase,
        IInstallRenoDXUseCase installRenoDXUseCase,
        IUninstallRenoDXUseCase uninstallRenoDXUseCase,
        IUpdateCheckService updateCheckService
    )
    {
        _logger = logger;
        _installReShadeUseCase = installReShadeUseCase;
        _uninstallReShadeUseCase = uninstallReShadeUseCase;
        _installRenoDXUseCase = installRenoDXUseCase;
        _uninstallRenoDXUseCase = uninstallRenoDXUseCase;
        _updateCheckService = updateCheckService;
    }

    public async Task<ModOperationResponse> InstallOrUpdateReShadeAsync(InstallReShadeRequest request,
        IProgress<DownloadProgressReport>? progress = null)
    {
        if (IsGamePathInvalid(request.Game))
            return new ModOperationResponse(false, request.Game,
                "Game path could not be found.\n\n" +
                "Please perform a rescan.");

        if (HasStaleReShadeRecord(request.Game, out var staleError))
            return staleError;

        try
        {
            var result = await _installReShadeUseCase.ExecuteAsync(request, progress);

            if (result.IsSuccess && result.UpdatedGame.ReShade is not null)
                return result with
                {
                    UpdateCheckResult = _updateCheckService.CheckReShadeUpdate(result.UpdatedGame.ReShade)
                };

            return result;
        }
        catch (Exception ex)
        {
            const string message = "Unexpected error occured while installing ReShade.";
            LogError(message, ex);
            return new ModOperationResponse(false, request.Game,
                message + " Check logs for more information.");
        }
    }


    public async Task<ModOperationResponse> UninstallReShadeAsync(Game game)
    {
        if (IsGamePathInvalid(game))
            return new ModOperationResponse(false, game,
                "Game path could not be found.\n\n" +
                "Please perform a rescan.");

        if (game.ReShade is null)
            return new ModOperationResponse(false, game,
                "No ReShade installation detected for this game. Please perform a full rescan.");

        try
        {
            return _uninstallReShadeUseCase.Execute(game);
        }
        catch (Exception ex)
        {
            const string message = "An unexpected error occured uninstalling ReShade.";
            LogError(message, ex);
            return new ModOperationResponse(false, game, message + " Check the logs for more details.");
        }
    }

    public async Task<RenoDXInstallResponse> InstallOrUpdateRenoDXAsync(RenoDXInstallRequest request,
        IProgress<DownloadProgressReport>? progress = null)
    {
        if (IsGamePathInvalid(request.Game))
            return new RenoDXInstallResponse(false, request.Game, request.GameEntry,
                "Game path could not be found.\n\n" +
                "Please perform a rescan.");

        if (HasStaleRenoDXRecord(request.Game))
            return new RenoDXInstallResponse(false, request.Game, request.GameEntry,
                "RenoDX with the recorded filename could not be found on disk.\n\n" +
                "Please perform a rescan.");

        try
        {
            var result = await _installRenoDXUseCase.ExecuteAsync(request, progress);

            return result;
        }
        catch (Exception ex)
        {
            const string message = "Unexpected error occured while installing RenoDX.";
            LogError(message, ex);
            return new RenoDXInstallResponse(false, request.Game, request.GameEntry,
                message + " Check logs for more information.");
        }
    }


    public async Task<RenoDXUninstallResponse> UninstallRenoDXAsync(Game game)
    {
        if (IsGamePathInvalid(game))
            return new RenoDXUninstallResponse(false, game, null,
                "Game path could not be found.\n\n" +
                "Please perform a rescan.");

        if (game.RenoDX is null)
            return new RenoDXUninstallResponse(false, game, null,
                "No RenoDX installation detected for this game. Please perform a full rescan.");

        try
        {
            return _uninstallRenoDXUseCase.Execute(game);
        }
        catch (Exception ex)
        {
            const string message = "An unexpected error occured uninstalling RenoDX.";
            LogError(message, ex);
            return new RenoDXUninstallResponse(false, game, null,
                message + " Check the logs for more details.");
        }
    }

    private static bool HasStaleReShadeRecord(Game game, out ModOperationResponse result)
    {
        if (game.ReShade is { } reShade &&
            !File.Exists(Path.Combine(game.ExecutablePath!, reShade.SelectedFilename)))
        {
            result = new ModOperationResponse(
                false,
                game,
                $"""
                 ReShade was recorded as {reShade.SelectedFilename} but that file no longer exists.
                 It may have been moved or renamed. Please perform a full library rescan.
                 """,
                true
            );
            return true;
        }

        result = null!;
        return false;
    }

    private static bool HasStaleRenoDXRecord(Game game)
    {
        if (game.RenoDX is { } renoDX &&
            !File.Exists(Path.Combine(game.ExecutablePath!, renoDX.SelectedName!)))
            return true;

        return false;
    }

    private static bool IsGamePathInvalid(Game game)
    {
        if (!string.IsNullOrWhiteSpace(game.ExecutablePath) && Directory.Exists(game.ExecutablePath))
        {
            return false;
        }

        return true;
    }
}
