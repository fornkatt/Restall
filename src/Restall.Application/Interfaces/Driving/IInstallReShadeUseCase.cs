using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Application.UseCases.Requests;

namespace Restall.Application.Interfaces.Driving;

public interface IInstallReShadeUseCase
{
    Task<ModOperationResultDto> ExecuteAsync(InstallReShadeRequest request, IProgress<DownloadProgressReportDto>? progress = null);
}