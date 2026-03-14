using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IGameDetectionService
{
    Task<GameScanResultDto> FindGames(IProgress<GameScanProgressReportDto>? progress = null);
}