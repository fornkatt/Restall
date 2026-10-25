// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.DTOs;
using Restall.Application.DTOs.Requests;
using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driving;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.Facades;

// TODO: combine with UseCases
public sealed class ModManagementFacade : IModManagementFacade
{
    private const string GamePathNotFoundMessage = "Game path could not be found.\n\nPlease perform a rescan.";

    private readonly IInstallReShadeUseCase _installReShadeUseCase;
    private readonly IUninstallReShadeUseCase _uninstallReShadeUseCase;
    private readonly IInstallRenoDXUseCase _installRenoDXUseCase;
    private readonly IUninstallRenoDXUseCase _uninstallRenoDXUseCase;

    public ModManagementFacade(
        IInstallReShadeUseCase installReShadeUseCase,
        IUninstallReShadeUseCase uninstallReShadeUseCase,
        IInstallRenoDXUseCase installRenoDXUseCase,
        IUninstallRenoDXUseCase uninstallRenoDXUseCase
    )
    {
        _installReShadeUseCase = installReShadeUseCase;
        _uninstallReShadeUseCase = uninstallReShadeUseCase;
        _installRenoDXUseCase = installRenoDXUseCase;
        _uninstallRenoDXUseCase = uninstallRenoDXUseCase;
    }

    public async Task<ModOperationResponse> InstallOrUpdateReShadeAsync(InstallReShadeRequest request,
        IProgress<DownloadProgressReport>? progress = null)
    {
        UIThreadGuard.AssertNotOnUIThread();

        if (IsGamePathInvalid(request.Game))
            return new ModOperationResponse(false, request.Game, GamePathNotFoundMessage);

        if (HasStaleReShadeRecord(request.Game, out var staleError))
            return staleError;

        return await _installReShadeUseCase.ExecuteAsync(request, progress);
    }


    public async Task<ModOperationResponse> UninstallReShadeAsync(Game game)
    {
        UIThreadGuard.AssertNotOnUIThread();

        if (IsGamePathInvalid(game))
            return new ModOperationResponse(false, game, GamePathNotFoundMessage);

        if (game.ReShade is null)
            return new ModOperationResponse(false, game,
                "No ReShade installation detected for this game. Please perform a full rescan.");

        return _uninstallReShadeUseCase.Execute(game);
    }

    public async Task<RenoDXInstallResponse> InstallOrUpdateRenoDXAsync(RenoDXInstallRequest request,
        IProgress<DownloadProgressReport>? progress = null)
    {
        UIThreadGuard.AssertNotOnUIThread();

        if (IsGamePathInvalid(request.Game))
            return new RenoDXInstallResponse(false, GamePathNotFoundMessage);

        return await _installRenoDXUseCase.ExecuteAsync(request, progress);
    }


    public async Task<RenoDXUninstallResponse> UninstallRenoDXAsync(Game game)
    {
        UIThreadGuard.AssertNotOnUIThread();

        if (IsGamePathInvalid(game))
            return new RenoDXUninstallResponse(false, GamePathNotFoundMessage);

        if (game.RenoDX is null)
            return new RenoDXUninstallResponse(false,
                "No RenoDX installation detected for this game. Please perform a full rescan.");

        return _uninstallRenoDXUseCase.Execute(game);
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
                 """
            );
            return true;
        }

        result = null!;
        return false;
    }

    private static bool IsGamePathInvalid(Game game) =>
        string.IsNullOrWhiteSpace(game.ExecutablePath) || !Directory.Exists(game.ExecutablePath);
}
