using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public sealed class UninstallReShadeUseCase : IUninstallReShadeUseCase
{
    private readonly IModInstallService _modInstallService;

    public UninstallReShadeUseCase(
        IModInstallService modInstallService
        )
    {
        _modInstallService = modInstallService;
    }

    public Task<ModOperationResultDto> ExecuteAsync(Game game) =>
        _modInstallService.UninstallReShadeAsync(game);
}