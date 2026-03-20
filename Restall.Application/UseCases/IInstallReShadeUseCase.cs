using Restall.Application.DTOs;
using Restall.Application.UseCases.Requests;

namespace Restall.Application.UseCases;

public interface IInstallReShadeUseCase
{
    Task<ModOperationResultDto> ExecuteAsync(InstallReShadeRequest request, IProgress<DownloadProgressReportDto>? progress = null);
}