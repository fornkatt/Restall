using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public interface IRefreshLibraryUseCase
{
    Task<RefreshLibraryResultDto> ExecuteFullRescanAsync(IProgress<GameScanProgressReportDto>? progress = null);
}

public interface ILightRefreshLibraryUseCase
{
    Task<RefreshLibraryResultDto> ExecuteLightRescanAsync(IReadOnlyList<Game> existingGames, IProgress<GameScanProgressReportDto>? progress = null);
}