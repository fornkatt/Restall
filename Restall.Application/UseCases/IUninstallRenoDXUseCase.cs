using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public interface IUninstallRenoDXUseCase
{
    Task<ModOperationResultDto> ExecuteAsync(Game game);
}