using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IPathService
{
    string GetReShadeCachePath(ReShade reShade);
    string GetRenoDXCachePath(RenoDX renoDx);
    string GetReShadeDownloadCacheDirectory(ReShade.Branch branch);
    string GetRenoDXDownloadCacheDirectory(RenoDX.Branch branch);

    string GetReShadeInstallerFilePath(ReShade.Branch branch, string version);
    string GetReShadeExtractedFilePath(ReShade reShade);
    
    string GetArtworkCacheDirectory();
    string GetGameArtworkCover(string slug);
    string GetGameArtThumbnailPath(string slug);

    IReadOnlyList<string> GetSteamLinuxPaths();
    string GetEpicInstallPath();
    string GetHeroicPath();
    string GetHeroicInstalledPath(Game.Platform platform);
    string GetHeroicStoreCache(Game.Platform platform, string destination);
    string GetDefaultLogPath();
    
    
}