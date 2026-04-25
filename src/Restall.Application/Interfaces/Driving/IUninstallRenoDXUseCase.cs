using Restall.Application.DTOs.Results;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driving;

public interface IUninstallRenoDXUseCase
{
    Task<ModOperationResultDto> ExecuteAsync(Game game);
}