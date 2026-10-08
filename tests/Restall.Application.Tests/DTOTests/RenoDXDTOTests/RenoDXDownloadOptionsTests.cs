// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

public class RenoDXDownloadOptionsTests
{
    private static readonly RenoDXTagInfo s_snapshot = new(new DateOnly(2026, 9, 28),
        RenoDX.Branch.Snapshot, new Uri("https://restalltests.com/snapshot/"), []);

    private static readonly RenoDXTagInfo s_nightly = new(new DateOnly(2026, 9, 28),
        RenoDX.Branch.Nightly, new Uri("https://restalltests.com/nightly/"), []);

    private static readonly Uri s_directUrl = new("https://restalltests.com/renodx-game.addon64");

    [Fact]
    public void Branches_AllSources_ReturnsSnapshotNightlyDirectInThatOrder()
    {
        var actual = CreateOptions(s_snapshot, [s_nightly], s_directUrl);

        Assert.Equal([RenoDX.Branch.Snapshot, RenoDX.Branch.Nightly, RenoDX.Branch.Direct], actual.Branches);
    }

    [Fact]
    public void Branches_OnlySnapshot_ReturnsOnlySnapshot()
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
    public void LatestNightly_NightliesInMixedDateOrder_ResturnsNewestNightly()
    {
        var newestNightly = CreateNightly(new DateOnly(2026, 10, 7));
        var olderNightly = CreateNightly(new DateOnly(2026, 10, 6));
        var oldestNightly = CreateNightly(new DateOnly(2026, 10, 5));

        var actual = CreateOptions(nightlies: [oldestNightly, newestNightly, olderNightly]);

        Assert.Same(newestNightly, actual.LatestNightly);
    }

    [Fact]
    public void LatestNightly_NoNightlies_ReturnsNothing()
    {
        var actual = CreateOptions(s_snapshot);

        Assert.Null(actual.LatestNightly);
    }

    private static RenoDXTagInfo CreateNightly(DateOnly date) =>
        new(date, RenoDX.Branch.Nightly, new Uri("https://restalltests.com/nightly/"), []);

    private static RenoDXDownloadOptions CreateOptions(RenoDXTagInfo? snapshot = null,
        ImmutableArray<RenoDXTagInfo>? nightlies = null, Uri? directUrl = null) =>
        new("renodx-game.addon64", directUrl, snapshot, nightlies ?? []);
}
