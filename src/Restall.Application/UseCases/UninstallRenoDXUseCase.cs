using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public sealed class UninstallRenoDXUseCase : IUninstallRenoDXUseCase
{
    private readonly IModInstallService _modInstallService;

    public UninstallRenoDXUseCase(
        IModInstallService modInstallService
        )
    {
        _modInstallService = modInstallService;
    }

    public Task<ModOperationResultDto> ExecuteAsync(Game game) =>
        _modInstallService.UninstallRenoDXAsync(game);
}