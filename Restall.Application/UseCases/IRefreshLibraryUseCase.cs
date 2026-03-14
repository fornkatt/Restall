using Restall.Application.DTOs;

namespace Restall.Application.UseCases;

public interface IRefreshLibraryUseCase
{
    Task<AppInitializationResultDto> ExecuteAsync(IProgress<GameScanProgressReportDto>? progress = null);
}