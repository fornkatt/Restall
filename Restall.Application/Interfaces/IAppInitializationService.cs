using Restall.Application.DTOs;

namespace Restall.Application.Interfaces;

public interface IAppInitializationService
{
    Task<AppInitializationResultDto> InitializeAsync(IProgress<GameScanProgressReportDto>? progress = null);
}