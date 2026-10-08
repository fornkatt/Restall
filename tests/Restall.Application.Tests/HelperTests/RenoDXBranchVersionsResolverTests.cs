// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Helpers;
using Restall.Domain.Entities;

namespace Restall.Application.Tests.HelperTests;

public class RenoDXBranchVersionsResolverTests
{
    private const string AddonFilename = "renodx-game.addon64";

    [Fact]
    public void Resolve_NoDownloadOptions_ReturnsNothing()
    {
        var installedRenoDX = new RenoDX { OriginalName = AddonFilename, Version = "20261007" };

        var actual = RenoDXBranchVersionsResolver.Resolve(installedRenoDX,
            null);

        Assert.Empty(actual);
    }

    [Fact]
    public void Resolve_InstalledFileOlderThanSnapshot_ReturnsUpdateOnlyOnNewerBranch()
    {
        var installedRenoDX = new RenoDX { OriginalName = AddonFilename, Version = "20261006" };

        var actual = RenoDXBranchVersionsResolver.Resolve(installedRenoDX,
            CreateOptions(true));

        Assert.Equivalent(new[] { RenoDX.Branch.Snapshot, RenoDX.Branch.Nightly, RenoDX.Branch.Direct },
            actual.Keys, strict: true);
        Assert.True(actual[RenoDX.Branch.Snapshot].UpdateAvailable);
        Assert.Equal("20261006", actual[RenoDX.Branch.Snapshot].InstalledVersion);
        Assert.Equal("20261007", actual[RenoDX.Branch.Snapshot].LatestVersion);
        Assert.False(actual[RenoDX.Branch.Nightly].UpdateAvailable);
        Assert.Equal("20261006", actual[RenoDX.Branch.Nightly].LatestVersion);
    }

    [Fact]
    public void Resolve_InstalledFileFromSameDayAsSnapshot_ReturnsNoSnapshotUpdate()
    {
        var installedRenoDX = new RenoDX { OriginalName = AddonFilename, Version = "20261007" };

        var actual = RenoDXBranchVersionsResolver.Resolve(installedRenoDX,
            CreateOptions(true));

        Assert.False(actual[RenoDX.Branch.Snapshot].UpdateAvailable);
    }

    [Fact]
    public void Resolve_OptionsForOtherFileThanInstalled_ReturnsNoInstalledVersion()
    {
        var installedRenoDX = new RenoDX { OriginalName = "renodx-game.addon32", Version = "20261001" };

        var actual = RenoDXBranchVersionsResolver.Resolve(installedRenoDX,
            CreateOptions(false));

        Assert.Null(actual[RenoDX.Branch.Snapshot].InstalledVersion);
        Assert.False(actual[RenoDX.Branch.Snapshot].UpdateAvailable);
        Assert.False(actual[RenoDX.Branch.Nightly].UpdateAvailable);
    }

    [Fact]
    public void Resolve_InstalledVersionWithoutBuildDate_ReturnsNoUpdate()
    {
        var installedRenoDX = new RenoDX { OriginalName = AddonFilename, Version = "1.2.3" };

        var actual = RenoDXBranchVersionsResolver.Resolve(installedRenoDX,
            CreateOptions(true));

        Assert.Equal("1.2.3", actual[RenoDX.Branch.Snapshot].InstalledVersion);
        Assert.False(actual[RenoDX.Branch.Snapshot].UpdateAvailable);
    }

    [Fact]
    public void Resolve_DirectBranch_ReturnsUpdateCheckNotSupported()
    {
        var installedRenoDX = new RenoDX { OriginalName = AddonFilename, Version = "20261001" };

        var actual = RenoDXBranchVersionsResolver.Resolve(installedRenoDX,
            CreateOptions(true));

        Assert.False(actual[RenoDX.Branch.Direct].IsSupported);
        Assert.False(actual[RenoDX.Branch.Direct].UpdateAvailable);
        Assert.Null(actual[RenoDX.Branch.Direct].LatestVersion);
        Assert.Equal("20261001", actual[RenoDX.Branch.Direct].InstalledVersion);
    }

    private static RenoDXDownloadOptions CreateOptions(bool isInstalledFile) =>
        new(AddonFilename, new Uri("https://restalltests.com/releases/renodx-game.addon64"),
            CreateTag(new DateOnly(2026, 10, 7), RenoDX.Branch.Snapshot),
            [
                CreateTag(new DateOnly(2026, 10, 6), RenoDX.Branch.Nightly),
                CreateTag(new DateOnly(2026, 10, 5), RenoDX.Branch.Nightly)
            ], isInstalledFile);

    private static RenoDXTagInfo CreateTag(DateOnly date, RenoDX.Branch branch) =>
        new(date, branch, new Uri("https://restalltests.com/releases/"), [AddonFilename]);
}
