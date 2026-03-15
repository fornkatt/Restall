using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Application.UseCases;
using Restall.Domain.Entities;

namespace Restall.Application.Services;

public class ModManagementFacade : IModManagementFacade
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
        var result = await _installReShadeUseCase.ExecuteAsync(request, progress);

        if (result.IsSuccess && result.UpdatedGame.ReShade is not null)
            return result with { UpdateCheckResult = _updateCheckService.CheckReShadeUpdate(result.UpdatedGame.ReShade) };

        return result;
    }
        

    public Task<ModOperationResultDto> UninstallReShadeAsync(Game game) =>
        _uninstallReShadeUseCase.ExecuteAsync(game);

    public async Task<ModOperationResultDto> InstallOrUpdateRenoDXAsync(InstallRenoDXRequest request,
        IProgress<DownloadProgressReportDto>? progress = null)
    {
        var result = await _installRenoDXUseCase.ExecuteAsync(request, progress);

        if (result.IsSuccess && result.UpdatedGame.RenoDX is not null)
            return result with { UpdateCheckResult = _updateCheckService.CheckRenoDXUpdate(result.UpdatedGame.RenoDX) };

        return result;
    }
        

    public Task<ModOperationResultDto> UninstallRenoDXAsync(Game game) =>
        _uninstallRenoDXUseCase.ExecuteAsync(game);
}