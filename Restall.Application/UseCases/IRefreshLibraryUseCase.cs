using Restall.Application.DTOs;

namespace Restall.Application.UseCases;

public interface IRefreshLibraryUseCase
{
    Task<RefreshLibraryResultDto> ExecuteAsync(IProgress<GameScanProgressReportDto>? progress = null);
}