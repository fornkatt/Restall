// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

public class RenoDXDownloadOptionsTests
{
    private static readonly RenoDXTagInfo s_snapshot = new(new DateOnly(2026, 9, 28),
        new Uri("https://restalltests.com/snapshot/"), [], []);

    private static readonly RenoDXTagInfo s_nightly = new(new DateOnly(2026, 9, 28),
        new Uri("https://restalltests.com/nightly/"), [], []);

    private static readonly Uri s_directUrl = new("https://restalltests.com/renodx-game.addon64");

    [Fact]
    public void Branches_AllBranchSourcesAvailable_ReturnsSnapshotNightlyDirectInExactOrder()
    {
        var actual = CreateOptions(s_snapshot, [s_nightly], s_directUrl);

        Assert.Equal([RenoDX.Branch.Snapshot, RenoDX.Branch.Nightly, RenoDX.Branch.Direct], actual.Branches);
    }

    [Fact]
    public void Branches_OnlySnapshotDownloadOptionAvailable_ReturnsOnlySnapshotBranchInBranchList()
    {
        var actual = CreateOptions(s_snapshot);

        Assert.Equal([RenoDX.Branch.Snapshot], actual.Branches);
    }

    [Fact]
    public void Branches_OnlyNightly_ReturnsOnlyNightly()
    {
        var actual = CreateOptions(nightlies: [s_nightly]);

        Assert.Equal([RenoDX.Branch.Nightly], actual.Branches);
    }

    [Fact]
    public void Branches_OnlyDirect_ReturnsOnlyDirect()
    {
        var actual = CreateOptions(directUrl: s_directUrl);

        Assert.Equal([RenoDX.Branch.Direct], actual.Branches);
    }

    [Fact]
    public void Branches_NoSource_ReturnsEmpty()
    {
        var actual = CreateOptions();

        Assert.Empty(actual.Branches);
    }

    [Fact]
    public void Nightlies_NightliesInMixedDateOrder_ReturnsLatestFirst()
    {
        var latestNightly = CreateNightly(new DateOnly(2026, 10, 7));
        var olderNightly = CreateNightly(new DateOnly(2026, 10, 6));
        var oldestNightly = CreateNightly(new DateOnly(2026, 10, 5));

        var actual = CreateOptions(nightlies: [oldestNightly, latestNightly, olderNightly]);

        Assert.Equal([latestNightly, olderNightly, oldestNightly], actual.Nightlies);
    }

    [Fact]
    public void LatestNightly_NightliesInMixedDateOrder_ReturnsNewestNightly()
    {
        var newestNightly = CreateNightly(new DateOnly(2026, 10, 7));
        var olderNightly = CreateNightly(new DateOnly(2026, 10, 6));
        var oldestNightly = CreateNightly(new DateOnly(2026, 10, 5));

        var actual = CreateOptions(nightlies: [oldestNightly, newestNightly, olderNightly]);

        Assert.Same(newestNightly, actual.LatestNightly);
    }

    [Fact]
    public void LatestNightly_NoNightlies_ReturnsNull()
    {
        var actual = CreateOptions(s_snapshot);

        Assert.Null(actual.LatestNightly);
    }

    [Fact]
    public void GetNightly_VersionListed_ReturnsThatNightly()
    {
        var requestedNightly = CreateNightly(new DateOnly(2026, 10, 7));

        var actual = CreateOptions(nightlies:
        [
            CreateNightly(new DateOnly(2026, 10, 8)),
            requestedNightly, CreateNightly(new DateOnly(2026, 10, 5))
        ]);

        Assert.Same(requestedNightly, actual.GetNightly("20261007"));
    }

    [Fact]
    public void GetNightly_VersionNotListed_ReturnsNull()
    {
        var actual = CreateOptions(nightlies: [CreateNightly(new DateOnly(2026, 10, 7))]);

        Assert.Null(actual.GetNightly("20261006"));
    }

    private static RenoDXTagInfo CreateNightly(DateOnly date) =>
        new(date, new Uri("https://restalltests.com/nightly/"), [], []);

    private static RenoDXDownloadOptions CreateOptions(RenoDXTagInfo? snapshot = null,
        ImmutableArray<RenoDXTagInfo>? nightlies = null, Uri? directUrl = null) =>
        new("renodx-game.addon64", directUrl, snapshot, nightlies ?? []);
}
