using Restall.Application.DTOs;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driving;

public interface IModManagementFacade
{
    Task<ModOperationResultDto> InstallOrUpdateReShadeAsync(InstallReShadeRequest request, IProgress<DownloadProgressReportDto>? progress = null);
    Task<ModOperationResultDto> InstallOrUpdateRenoDXAsync(InstallRenoDXRequest request, IProgress<DownloadProgressReportDto>? progress = null);
    Task<ModOperationResultDto> UninstallReShadeAsync(Game game);
    Task<ModOperationResultDto> UninstallRenoDXAsync(Game game);
}