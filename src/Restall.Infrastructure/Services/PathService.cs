using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

internal sealed class PathService : IPathService
{
    private const string s_appName = "Restall";

    private static readonly string s_baseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), s_appName);
    private static readonly string s_userProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private static readonly string s_commonAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    
    private const string s_downloadCacheFolderName = "DownloadCache";
    private const string s_cacheFolderName = "Cache";
    private const string s_artworkFolderName = "Artwork";
    
    private const string s_iconFileName = "icon.png";
    private const string s_gameCoverFileName = "cover.png";
    
    private readonly string _defaultLogPath = Path.Combine(s_baseDirectory, "Logs");
    private readonly string _reShadeCacheBaseDir = Path.Combine(s_baseDirectory, s_cacheFolderName, "ReShade");
    private readonly string _reShadeDownloadCacheBaseDir = Path.Combine(s_baseDirectory, s_downloadCacheFolderName, "ReShade");
    private readonly string _renoDXDownloadCacheBaseDir = Path.Combine(s_baseDirectory, s_downloadCacheFolderName, "RenoDX");
    private readonly string _artworkCacheBaseDir = Path.Combine(s_baseDirectory, s_cacheFolderName, s_artworkFolderName);
    
    
    public string GetArtworkCacheDirectory() => _artworkCacheBaseDir;
    public string GetGameArtworkCover(string slug) => Path.Combine(_artworkCacheBaseDir,slug,  s_gameCoverFileName);
    public string GetGameArtThumbnailPath(string slug) => Path.Combine(_artworkCacheBaseDir, slug, s_iconFileName);
    public string GetEpicInstallPath() => Path.Combine(
        s_commonAppDataDirectory, "Epic", "EpicGamesLauncher", "Data", "Manifests");

    public string GetEpicHeroicPath() => OperatingSystem.IsWindows()
        ? Path.Combine(s_userProfileDirectory, "AppData", "Roaming", "heroic", "legendaryConfig", "legendary")
        : Path.Combine(s_userProfileDirectory, ".config", "heroic", "legendaryConfig", "legendary");

    public string GetGOGHeroicPath() => OperatingSystem.IsWindows()
        ? Path.Combine(s_userProfileDirectory, "AppData", "Roaming", "heroic", "gog_store")
        : Path.Combine(s_userProfileDirectory, ".config", "heroic", "gog_store");
    

    public string GetReShadeCachePath(ReShade reShade) =>
        Path.Combine(_reShadeCacheBaseDir, reShade.BranchName.ToString(), reShade.Version!);

    public string GetRenoDXCachePath(RenoDX renoDx) =>
        Path.Combine(_renoDXDownloadCacheBaseDir, renoDx.BranchName.ToString(), renoDx.OriginalName!);

    public string GetReShadeDownloadCachePath(ReShade.Branch branch) =>
        Path.Combine(_reShadeDownloadCacheBaseDir, branch.ToString());

    public string GetRenoDXDownloadCachePath(RenoDX.Branch branch) =>
        Path.Combine(_renoDXDownloadCacheBaseDir, branch.ToString());

    public string GetReShadeInstallerFilePath(ReShade.Branch branch, string version) =>
        Path.Combine(GetReShadeDownloadCachePath(branch), $"ReShade_Setup_{version}_Addon.exe");

    public string GetReShadeExtractedFilePath(ReShade reShade) =>
        Path.Combine(GetReShadeCachePath(reShade), reShade.OriginalFileName);

    public string GetDefaultLogPath() => _defaultLogPath;
}