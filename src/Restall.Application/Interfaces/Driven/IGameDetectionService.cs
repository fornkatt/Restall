using Restall.Application.DTOs;

namespace Restall.Application.Interfaces.Driven;

public interface IGameDetectionService
{
    Task<GameScanResultDto> FindGamesAsync(IProgress<GameScanProgressReportDto>? progress = null);
}