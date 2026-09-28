// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

public class RenoDXAddonFileTests
{
    private static readonly RenoDXTagInfoDto s_snapshot = new(new DateOnly(2026, 9, 28),
        RenoDX.Branch.Snapshot);

    private static readonly RenoDXTagInfoDto s_nightly = new(new DateOnly(2026, 9, 28),
        RenoDX.Branch.Nightly);

    private static readonly Uri s_directUrl = new("https://restalltests.com/renodx-game.addon64");

    [Fact]
    public void Branches_AllSources_ReturnsSnapshotNightlyDirectInThatOrder()
    {
        var file = CreateFile(s_snapshot, [s_nightly], s_directUrl);

        Assert.Equal([RenoDX.Branch.Snapshot, RenoDX.Branch.Nightly, RenoDX.Branch.Direct], file.Branches);
    }

    [Fact]
    public void Branches_OnlySnapshot_ReturnsOnlySnapshot()
    {
        var file = CreateFile(s_snapshot);

        Assert.Equal([RenoDX.Branch.Snapshot], file.Branches);
    }

    [Fact]
    public void Branches_OnlyNightly_ReturnsOnlyNightly()
    {
        var file = CreateFile(nightlies: [s_nightly]);

        Assert.Equal([RenoDX.Branch.Nightly], file.Branches);
    }

    [Fact]
    public void Branches_OnlyDirect_ReturnsOnlyDirect()
    {
        var file = CreateFile(directUrl: s_directUrl);

        Assert.Equal([RenoDX.Branch.Direct], file.Branches);
    }

    [Fact]
    public void Branches_NoSource_ReturnsEmpty()
    {
        var file = CreateFile();

        Assert.Empty(file.Branches);
    }

    private static RenoDXAddonFile CreateFile(RenoDXTagInfoDto? snapshot = null,
        ImmutableArray<RenoDXTagInfoDto>? nightlies = null, Uri? directUrl = null) =>
        new("renodx-game.addon64", directUrl, snapshot, nightlies ?? []);
}
