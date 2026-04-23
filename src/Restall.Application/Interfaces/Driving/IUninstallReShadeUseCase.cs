using Restall.Application.DTOs.Results;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driving;

public interface IUninstallReShadeUseCase
{
    Task<ModOperationResultDto> ExecuteAsync(Game game);
}