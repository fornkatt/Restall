using Restall.Application.DTOs;
using Restall.Application.UseCases.Requests;

namespace Restall.Application.UseCases;

public interface IInstallRenoDXUseCase
{
    Task<ModOperationResultDto> ExecuteAsync(InstallRenoDXRequest request, IProgress<DownloadProgressReportDto>? progress = null);
}