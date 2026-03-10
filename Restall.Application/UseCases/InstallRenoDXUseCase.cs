using Restall.Application.DTOs;

namespace Restall.Application.UseCases;

public class InstallRenoDXUseCase : IInstallRenoDXUseCase
{
    public Task<ModOperationResultDto> ExecuteAsync(InstallRenoDXRequest request, IProgress<DownloadProgressReportDto>? progress = null) =>
        Task.FromResult(new ModOperationResultDto(false, request.Game, "RenoDX installation is not implemented yet."));
}