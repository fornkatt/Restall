using Restall.Domain.Entities;
using Restall.Infrastructure.Services;

namespace Restall.Tests.Infrastructure;

public sealed class PathServiceTests
{
    // Verifies that SteamGridDB cache paths use the expected cache directory and artwork filenames.
    [Fact]
    public void SgdbPaths_ReturnExpectedCachePaths()
    {
        var sut = new PathService();

        Assert.EndsWith(Path.Combine("Restall", "Cache", "SGDB"), sut.GetSgdbCacheDirectory());
        Assert.EndsWith(Path.Combine("Restall", "Cache", "SGDB", "42", "banner.png"), sut.GetSgdbBannerPath(42));
        Assert.EndsWith(Path.Combine("Restall", "Cache", "SGDB", "42", "icon.png"), sut.GetSgdbThumbnailPath(42));
        Assert.EndsWith(Path.Combine("Restall", "Cache", "SGDB", "42", "logo.png"), sut.GetSgdbLogoPath(42));
    }

    // Verifies that ReShade cache and installer paths include branch, version and original filename.
    [Fact]
    public void ReShadePaths_ReturnExpectedCacheAndDownloadPaths()
    {
        var sut = new PathService();
        var reShade = new ReShade
        {
            BranchName = ReShade.Branch.Stable,
            Version = "6.5.0",
            Arch = ReShade.Architecture.x32
        };

        Assert.EndsWith(Path.Combine("Restall", "Cache", "ReShade", "Stable", "6.5.0"), sut.GetReShadeCachePath(reShade));
        Assert.EndsWith(Path.Combine("Restall", "DownloadCache", "ReShade", "Stable"), sut.GetReShadeDownloadCachePath(ReShade.Branch.Stable));
        Assert.EndsWith(Path.Combine("Restall", "DownloadCache", "ReShade", "Stable", "ReShade_Setup_6.5.0_Addon.exe"), sut.GetReShadeInstallerFilePath(ReShade.Branch.Stable, "6.5.0"));
        Assert.EndsWith(Path.Combine("Restall", "Cache", "ReShade", "Stable", "6.5.0", "ReShade32.dll"), sut.GetReShadeExtractedFilePath(reShade));
    }

    // Verifies that RenoDX cache and download paths include branch and original addon name.
    [Fact]
    public void RenoDXPaths_ReturnExpectedCacheAndDownloadPaths()
    {
        var sut = new PathService();
        var renoDx = new RenoDX
        {
            BranchName = RenoDX.Branch.Nightly,
            OriginalName = "renodx-unrealengine.addon64"
        };

        Assert.EndsWith(Path.Combine("Restall", "DownloadCache", "RenoDX", "Nightly", "renodx-unrealengine.addon64"), sut.GetRenoDXCachePath(renoDx));
        Assert.EndsWith(Path.Combine("Restall", "DownloadCache", "RenoDX", "Snapshot"), sut.GetRenoDXDownloadCachePath(RenoDX.Branch.Snapshot));
    }

    // Verifies that the default log directory is under the Restall local app data folder.
    [Fact]
    public void GetDefaultLogPath_ReturnsRestallLogsDirectory()
    {
        var sut = new PathService();

        Assert.EndsWith(Path.Combine("Restall", "Logs"), sut.GetDefaultLogPath());
    }
}
