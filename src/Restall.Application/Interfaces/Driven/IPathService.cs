using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IPathService
{
    string GetReShadeCachePath(ReShade reShade);
    string GetRenoDXCachePath(RenoDX renoDx);
    string GetReShadeDownloadCachePath(ReShade.Branch branch);
    string GetRenoDXDownloadCachePath(RenoDX.Branch branch);

    string GetReShadeInstallerFilePath(ReShade.Branch branch, string version);
    string GetReShadeExtractedFilePath(ReShade reShade);
    
    string GetSgdbBannerPath(int steamGridDbId);
    string GetSgdbThumbnailPath(int steamGridDbId);
    string GetSgdbLogoPath(int steamGridDbId);
    string GetSgdbCacheDirectory();

    string GetArtworkCacheDirectory();
    string GetGameArtworkCover(string slug);
    string GetGameArtThumbnailPath(string slug);

    string GetDefaultLogPath();
}