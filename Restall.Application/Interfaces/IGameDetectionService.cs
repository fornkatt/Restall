using Restall.Application.DTOs;

namespace Restall.Application.Interfaces;

public interface IGameDetectionService
{
    Task<GameScanResultDto> FindGames(IProgress<GameScanProgressReportDto>? progress = null);
}