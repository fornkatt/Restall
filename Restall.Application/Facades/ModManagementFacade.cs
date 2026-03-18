using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Application.UseCases;
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

    public ModManagementFacade(
        IInstallReShadeUseCase installReShadeUseCase,
        IUninstallReShadeUseCase uninstallReShadeUseCase,
        IInstallRenoDXUseCase installRenoDXUseCase,
        IUninstallRenoDXUseCase uninstallRenoDXUseCase,
        IUpdateCheckService updateCheckService
        )
    {
        _installReShadeUseCase = installReShadeUseCase;
        _uninstallReShadeUseCase= uninstallReShadeUseCase;
        _installRenoDXUseCase= installRenoDXUseCase;
        _uninstallRenoDXUseCase = uninstallRenoDXUseCase;
        _updateCheckService = updateCheckService;
    }

    public async Task<ModOperationResultDto> InstallOrUpdateReShadeAsync(InstallReShadeRequest request,
        IProgress<DownloadProgressReportDto>? progress = null)
    {
        if (IsGamePathInvalid(request.Game, out var error))
            return error;

        var result = await _installReShadeUseCase.ExecuteAsync(request, progress);

        if (result.IsSuccess && result.UpdatedGame.ReShade is not null)
            return result with { UpdateCheckResult = _updateCheckService.CheckReShadeUpdate(result.UpdatedGame.ReShade) };

        return result;
    }
        

    public async Task<ModOperationResultDto> UninstallReShadeAsync(Game game)
    {
        if (IsGamePathInvalid(game, out var error))
            return error;

        if (game.ReShade is null)
            return new ModOperationResultDto(false, game, "No ReShade installation detected for this game. Please perform a full rescan.");

        return await _uninstallReShadeUseCase.ExecuteAsync(game);
    }

    public async Task<ModOperationResultDto> InstallOrUpdateRenoDXAsync(InstallRenoDXRequest request,
        IProgress<DownloadProgressReportDto>? progress = null)
    {
        if (IsGamePathInvalid(request.Game, out var error))
            return error;

        var result = await _installRenoDXUseCase.ExecuteAsync(request, progress);

        if (result.IsSuccess && result.UpdatedGame.RenoDX is not null)
            return result with { UpdateCheckResult = _updateCheckService.CheckRenoDXUpdate(result.UpdatedGame.RenoDX) };

        return result;
    }
        

    public async Task<ModOperationResultDto> UninstallRenoDXAsync(Game game)
    {
        if (IsGamePathInvalid(game, out var error))
            return error;

        if (game.RenoDX is null)
            return new ModOperationResultDto(false, game, "No RenoDX installation detected for this game. Please perform a full rescan.");

        return await _uninstallRenoDXUseCase.ExecuteAsync(game);
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