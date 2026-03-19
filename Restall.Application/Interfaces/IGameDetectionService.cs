using Restall.Application.DTOs;

namespace Restall.Application.Interfaces;

public interface IGameDetectionService
{
    Task<GameScanResultDto> FindGamesAsync(IProgress<GameScanProgressReportDto>? progress = null);
}