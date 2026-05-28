using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.Facades;

public sealed class ModManagementFacade : IModManagementFacade
{
    private readonly IInstallReShadeUseCase _installReShadeUseCase;
    private readonly IUninstallReShadeUseCase _uninstallReShadeUseCase;
    private readonly IInstallRenoDXUseCase _installRenoDXUseCase;
    private readonly IUninstallRenoDXUseCase _uninstallRenoDXUseCase;
    private readonly IUpdateCheckService _updateCheckService;
    private readonly ILogService _logService;

    public ModManagementFacade(
        IInstallReShadeUseCase installReShadeUseCase,
        IUninstallReShadeUseCase uninstallReShadeUseCase,
        IInstallRenoDXUseCase installRenoDXUseCase,
        IUninstallRenoDXUseCase uninstallRenoDXUseCase,
        IUpdateCheckService updateCheckService,
        ILogService logService
    )
    {
        _installReShadeUseCase = installReShadeUseCase;
        _uninstallReShadeUseCase = uninstallReShadeUseCase;
        _installRenoDXUseCase = installRenoDXUseCase;
        _uninstallRenoDXUseCase = uninstallRenoDXUseCase;
        _updateCheckService = updateCheckService;
        _logService = logService;
    }

    public async Task<ModOperationResultDto> InstallOrUpdateReShadeAsync(InstallReShadeRequest request,
        IProgress<DownloadProgressReportDto>? progress = null)
    {
        if (IsGamePathInvalid(request.Game, out var error))
            return error;

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
            await _logService.LogErrorAsync(message, ex);
            return new ModOperationResultDto(false, request.Game,
                message + " Check logs for more information.");
        }
    }


    public async Task<ModOperationResultDto> UninstallReShadeAsync(Game game)
    {
        if (IsGamePathInvalid(game, out var error))
            return error;

        if (game.ReShade is null)
            return new ModOperationResultDto(false, game,
                "No ReShade installation detected for this game. Please perform a full rescan.");

        try
        {
            return await _uninstallReShadeUseCase.ExecuteAsync(game);
        }
        catch (Exception ex)
        {
            const string message = "An unexpected error occured uninstalling ReShade.";
            await _logService.LogErrorAsync(message, ex);
            return new ModOperationResultDto(false, game, message + " Check the logs for more details.");
        }
    }

    public async Task<ModOperationResultDto> InstallOrUpdateRenoDXAsync(InstallRenoDXRequest request,
        IProgress<DownloadProgressReportDto>? progress = null)
    {
        if (IsGamePathInvalid(request.Game, out var error))
            return error;

        if (HasStaleRenoDXRecord(request.Game, out var staleError))
            return staleError;

        try
        {
            var result = await _installRenoDXUseCase.ExecuteAsync(request, progress);

            if (result.IsSuccess && result.UpdatedGame.RenoDX is not null)
                return result with
                {
                    UpdateCheckResult = _updateCheckService.CheckRenoDXUpdate(result.UpdatedGame.RenoDX)
                };

            return result;
        }
        catch (Exception ex)
        {
            const string message = "Unexpected error occured while installing RenoDX.";
            await _logService.LogErrorAsync(message, ex);
            return new ModOperationResultDto(false, request.Game,
                message + " Check logs for more information.");
        }
    }


    public async Task<ModOperationResultDto> UninstallRenoDXAsync(Game game)
    {
        if (IsGamePathInvalid(game, out var error))
            return error;

        if (game.RenoDX is null)
            return new ModOperationResultDto(false, game,
                "No RenoDX installation detected for this game. Please perform a full rescan.");

        try
        {
            return await _uninstallRenoDXUseCase.ExecuteAsync(game);
        }
        catch (Exception ex)
        {
            const string message = "An unexpected error occured uninstalling RenoDX.";
            await _logService.LogErrorAsync(message, ex);
            return new ModOperationResultDto(false, game, message + " Check the logs for more details.");
        }
    }

    private static bool HasStaleReShadeRecord(Game game, out ModOperationResultDto result)
    {
        if (game.ReShade is { } reShade &&
            !File.Exists(Path.Combine(game.ExecutablePath!, reShade.SelectedFilename)))
        {
            result = new ModOperationResultDto(
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

    private static bool HasStaleRenoDXRecord(Game game, out ModOperationResultDto result)
    {
        if (game.RenoDX is { } renoDX &&
            !File.Exists(Path.Combine(game.ExecutablePath!, renoDX.SelectedName!)))
        {
            result = new ModOperationResultDto(
                false,
                game,
                $"""
                 RenoDX was recorded as {renoDX.SelectedName} but that file no longer exists.
                 It may have been moved or renamed. Please perform a full library rescan.
                 """,
                true
            );
            return true;
        }

        result = null!;
        return false;
    }

    private static bool IsGamePathInvalid(Game game, out ModOperationResultDto result)
    {
        if (!string.IsNullOrWhiteSpace(game.ExecutablePath) && Directory.Exists(game.ExecutablePath))
        {
            result = null!;
            return false;
        }

        result = new ModOperationResultDto(false, game,
            "Game folder not found. Please rescan your library.");
        return true;
    }
}