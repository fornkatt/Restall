using Restall.Application.DTOs;

namespace Restall.Application.UseCases;

public interface IInstallRenoDXUseCase
{
    Task<ModOperationResultDto> ExecuteAsync(InstallRenoDXRequest request, IProgress<DownloadProgressReportDto>? progress = null);
}