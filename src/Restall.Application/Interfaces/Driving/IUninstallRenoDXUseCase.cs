using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driving;

public interface IUninstallRenoDXUseCase
{
    Task<ModOperationResultDto> ExecuteAsync(Game game);
}