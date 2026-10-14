// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Helpers;
using Restall.Domain.Entities;

namespace Restall.Application.Tests.HelperTests;

public class RenoDXReleaseNotesResolverTests
{
    private const string AddonFilename = "renodx-game.addon64";

    [Fact]
    public void Resolve_SnapshotInstalled_ReturnsSnapshotNotesMarkedInstalledAndLatest()
    {
        var installedRenoDX = new RenoDX { OriginalName = AddonFilename, Version = "20261007" };

        var actual = RenoDXReleaseNotesResolver.Resolve(installedRenoDX, CreateOptions());

        var snapshotNotes = Assert.Single(actual[RenoDX.Branch.Snapshot]);
        Assert.Equal("20261007", snapshotNotes.Version);
        Assert.True(snapshotNotes.IsInstalled);
        Assert.True(snapshotNotes.IsLatest);
        Assert.Equal(["Snapshot change"], snapshotNotes.Lines);
    }

    [Fact]
    public void Resolve_OlderNightlyInstalled_ReturnsNightlyCommitNotesFromLatestToCurrentlyInstalled()
    {
        var installedRenoDX = new RenoDX { OriginalName = AddonFilename, Version = "20261005" };

        var actual = RenoDXReleaseNotesResolver.Resolve(installedRenoDX, CreateOptions());
        var nightlyNotes = actual[RenoDX.Branch.Nightly];

        Assert.Equal(["20261006", "20261005"], nightlyNotes.Select(n => n.Version));
        Assert.False(nightlyNotes[0].IsInstalled);
        Assert.True(nightlyNotes[0].IsLatest);
        Assert.True(nightlyNotes[1].IsInstalled);
        Assert.False(nightlyNotes[1].IsLatest);
    }

    [Fact]
    public void Resolve_NothingInstalled_ReturnsLatestNightlyOnly()
    {
        var actual = RenoDXReleaseNotesResolver.Resolve(null, CreateOptions());
        var nightlyNotes = Assert.Single(actual[RenoDX.Branch.Nightly]);

        Assert.Equal("20261006", nightlyNotes.Version);
        Assert.False(nightlyNotes.IsInstalled);
    }

    [Fact]
    public void Resolve_DirectBranch_ReturnsNothing()
    {
        var actual = RenoDXReleaseNotesResolver.Resolve(null, CreateOptions());

        Assert.Empty(actual[RenoDX.Branch.Direct]);
    }

    [Fact]
    public void Resolve_InstalledNewerThanLatestNightly_ReturnsLatestNightlyOnly()
    {
        var installedRenoDX = new RenoDX { OriginalName = AddonFilename, Version = "20261007" };

        var actual = RenoDXReleaseNotesResolver.Resolve(installedRenoDX, CreateOptions());
        var nightlyNotes = Assert.Single(actual[RenoDX.Branch.Nightly]);

        Assert.Equal("20261006", nightlyNotes.Version);
        Assert.False(nightlyNotes.IsInstalled);
    }

    private static RenoDXDownloadOptions CreateOptions() =>
        new(AddonFilename, new Uri("https://restalltests.com/releases/renodx-game.addon64"),
            CreateTag(new DateOnly(2026, 10, 7), "Snapshot change"),
            [
                CreateTag(new DateOnly(2026, 10, 5), "Older nightly change"),
                CreateTag(new DateOnly(2026, 10, 6), "Latest nightly change"),
                CreateTag(new DateOnly(2026, 10, 4), "Oldest nightly change")
            ]);

    private static RenoDXTagInfo CreateTag(DateOnly date, string commitNote) =>
        new(date, new Uri("https://restalltests.com/releases/"), [AddonFilename], [commitNote]);
}
