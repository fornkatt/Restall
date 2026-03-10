using Restall.Application.DTOs;

namespace Restall.Application.UseCases;

public interface IInstallReShadeUseCase
{
    Task<ModOperationResultDto> ExecuteAsync(InstallReShadeRequest request, IProgress<DownloadProgressReportDto>? progress = null);
}