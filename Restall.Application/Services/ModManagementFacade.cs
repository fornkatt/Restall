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

    public ModManagementFacade(
        IInstallReShadeUseCase installReShadeUseCase,
        IUninstallReShadeUseCase uninstallReShadeUseCase,
        IInstallRenoDXUseCase installRenoDXUseCase,
        IUninstallRenoDXUseCase uninstallRenoDXUseCase
        )
    {
        _installReShadeUseCase = installReShadeUseCase;
        _uninstallReShadeUseCase= uninstallReShadeUseCase;
        _installRenoDXUseCase= installRenoDXUseCase;
        _uninstallRenoDXUseCase = uninstallRenoDXUseCase;
    }

    public Task<ModOperationResultDto> InstallOrUpdateReShadeAsync(InstallReShadeRequest request,
        IProgress<DownloadProgressReportDto>? progress = null) =>
        _installReShadeUseCase.ExecuteAsync(request, progress);

    public Task<ModOperationResultDto> UninstallReShadeAsync(Game game) =>
        _uninstallReShadeUseCase.ExecuteAsync(game);

    public Task<ModOperationResultDto> InstallOrUpdateRenoDXAsync(InstallRenoDXRequest request,
        IProgress<DownloadProgressReportDto>? progress = null) =>
        _installRenoDXUseCase.ExecuteAsync(request, progress);

    public Task<ModOperationResultDto> UninstallRenoDXAsync(Game game) =>
        _uninstallRenoDXUseCase.ExecuteAsync(game);
}