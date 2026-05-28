using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;

namespace Restall.Application.Interfaces.Driven;

public interface IGameDetectionService
{
    Task<GameScanResultDto> FindGamesAsync(IProgress<GameScanProgressReportDto>? progress = null);
}