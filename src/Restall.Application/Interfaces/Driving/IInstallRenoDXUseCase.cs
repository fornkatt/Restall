using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Application.UseCases.Requests;

namespace Restall.Application.Interfaces.Driving;

public interface IInstallRenoDXUseCase
{
    Task<ModOperationResultDto> ExecuteAsync(InstallRenoDXRequest request, IProgress<DownloadProgressReportDto>? progress = null);
}