// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IPathService
{
    string GetReShadeCachePath(ReShade reShade);
    string GetRenoDXCachePath(RenoDX renoDx);
    string GetRenoDXDownloadCacheDirectory(RenoDX.Branch branch);
    string GetRenoDXSnapshotDownloadPath(string snapshotVersion, string addonFilename);
    string GetRenoDXNightlyDownloadPath(string nightlyVersion, string addonFilename);
    string GetRenoDXDirectDownloadPath(string addonFilename);

    string GetReShadeDownloadCacheDirectory(ReShade.Branch branch);
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
