using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface ICachePathService
{
    string GetReShadeCachePath(ReShade reShade);
    string GetRenoDXCachePath(RenoDX renoDx);
    string GetReShadeDownloadCachePath(ReShade.Branch branch);
    string GetRenoDXDownloadCachePath(RenoDX.Branch branch);

    string GetReShadeInstallerFilePath(ReShade.Branch branch, string version);
    string GetReShadeExtractedFilePath(ReShade reShade);
}