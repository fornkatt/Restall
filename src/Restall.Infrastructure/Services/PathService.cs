// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

internal sealed class PathService : IPathService
{
    private const string AppName = "Restall";

    private const string DownloadCacheFolderName = "DownloadCache";
    private const string CacheFolderName = "Cache";
    private const string ArtworkFolderName = "Artwork";

    private const string IconFileName = "icon.png";
    private const string GameCoverFileName = "cover.png";

    private static readonly string s_baseDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);

    private static readonly string s_userProfileDirectory =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    private static readonly string s_commonAppDataDirectory =
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

    private readonly string _defaultLogPath = Path.Combine(s_baseDirectory, "Logs");
    private readonly string _reShadeCacheBaseDir = Path.Combine(s_baseDirectory, CacheFolderName, "ReShade");

    private readonly string _reShadeDownloadCacheBaseDir =
        Path.Combine(s_baseDirectory, DownloadCacheFolderName, "ReShade");

    private readonly string _renoDXDownloadCacheBaseDir =
        Path.Combine(s_baseDirectory, DownloadCacheFolderName, "RenoDX");

    private readonly string _artworkCacheBaseDir =
        Path.Combine(s_baseDirectory, CacheFolderName, ArtworkFolderName);

    public string GetArtworkCacheDirectory() => _artworkCacheBaseDir;
    public string GetGameArtworkCover(string slug) => Path.Combine(_artworkCacheBaseDir, slug, GameCoverFileName);
    public string GetGameArtThumbnailPath(string slug) => Path.Combine(_artworkCacheBaseDir, slug, IconFileName);

    public IReadOnlyList<string> GetSteamLinuxPaths() =>
    [
        Path.Combine(s_userProfileDirectory, ".steam", "steam"),
        Path.Combine(s_userProfileDirectory, ".local", "share", "Steam"),
        Path.Combine(s_userProfileDirectory, "snap", "steam", "common", ".local", "share", "Steam")
    ];

    public string GetEpicInstallPath() =>
        Path.Combine(s_commonAppDataDirectory, "Epic", "EpicGamesLauncher", "Data", "Manifests");

    public string GetHeroicPath() => OperatingSystem.IsWindows()
        ? Path.Combine(s_userProfileDirectory, "AppData", "Roaming", "heroic")
        : Path.Combine(s_userProfileDirectory, ".config", "heroic");

    public string GetHeroicInstalledPath(Game.Platform platform) =>
        (GetHeroicPath() is { } root
            ? platform switch
            {
                Game.Platform.Epic => Path.Combine(root, "legendaryConfig", "legendary", "installed.json"),
                Game.Platform.GOG => Path.Combine(root, "gog_store", "installed.json"),
                _ => null
            }
            : null) ?? "Unknown";

    public string GetHeroicStoreCache(Game.Platform platform, string destination) =>
        (GetHeroicPath() is { } root
            ? platform switch
            {
                Game.Platform.Epic => Path.Combine(root, "store_cache", destination),
                Game.Platform.GOG => Path.Combine(root, "store_cache", destination),
                _ => null
            }
            : null) ?? "Unknown";


    public string GetReShadeCachePath(ReShade reShade) =>
        Path.Combine(_reShadeCacheBaseDir, reShade.BranchName.ToString(), reShade.Version!);

    public string GetRenoDXCachePath(RenoDX renoDx) =>
        Path.Combine(_renoDXDownloadCacheBaseDir, renoDx.BranchName.ToString(), renoDx.OriginalName!);

    public string GetReShadeDownloadCacheDirectory(ReShade.Branch branch) =>
        Path.Combine(_reShadeDownloadCacheBaseDir, branch.ToString());

    public string GetRenoDXDownloadCacheDirectory(RenoDX.Branch branch) =>
        Path.Combine(_renoDXDownloadCacheBaseDir, branch.ToString());

    public string GetReShadeInstallerFilePath(ReShade.Branch branch, string version) =>
        Path.Combine(GetReShadeDownloadCacheDirectory(branch), $"ReShade_Setup_{version}_Addon.exe");

    public string GetReShadeExtractedFilePath(ReShade reShade) =>
        Path.Combine(GetReShadeCachePath(reShade), reShade.OriginalFileName);

    public string GetDefaultLogPath() => _defaultLogPath;
}
