// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Helpers;
using Restall.Domain.Entities;
using System.Collections.Frozen;

namespace Restall.Application.Tests.HelperTests;

public class RenoDXAddonFileResolverTests
{
    private const string AddonFilename = "renodx-game.addon64";

    private static readonly Uri s_directUrl = new("https://restalltests.com/direct/renodx-game.addon64");

    [Fact]
    public void Resolve_FileOnSnapshotAndNightlyWithDirectUrl_ReturnsAllBranches()
    {
        var snapshot = CreateTag(7, RenoDX.Branch.Snapshot, AddonFilename);
        var nightly = CreateTag(6, RenoDX.Branch.Nightly, AddonFilename);

        var actual = RenoDXAddonFileResolver.Resolve(AddonFilename, s_directUrl, snapshot,
            [nightly]);

        Assert.NotNull(actual);
        Assert.Equal(AddonFilename, actual.Filename);
        Assert.Equal(s_directUrl, actual.DirectUrl);
        Assert.Same(snapshot, actual.Snapshot);
        Assert.Same(nightly, Assert.Single(actual.Nightlies));
        Assert.Equal([RenoDX.Branch.Snapshot, RenoDX.Branch.Nightly, RenoDX.Branch.Direct], actual.Branches);
    }

    [Fact]
    public void Resolve_FileOnOneOfTwoNightlies_ReturnsOnlyNightlyWithFile()
    {
        var nightlyWithFile = CreateTag(7, RenoDX.Branch.Nightly, AddonFilename);
        var nightlyWithoutFile = CreateTag(6, RenoDX.Branch.Nightly,
            "renodx-other.addon64");

        var actual = RenoDXAddonFileResolver.Resolve(AddonFilename, null, null,
            [nightlyWithFile, nightlyWithoutFile]);

        Assert.NotNull(actual);
        Assert.Same(nightlyWithFile, Assert.Single(actual.Nightlies));
        Assert.Equal([RenoDX.Branch.Nightly], actual.Branches);
    }

    [Fact]
    public void Resolve_FileMissingFromSnapshotList_ReturnsNoSnapshot()
    {
        var snapshot = CreateTag(7, RenoDX.Branch.Snapshot, "renodx-other.addon64");

        var actual = RenoDXAddonFileResolver.Resolve(AddonFilename, s_directUrl, snapshot, []);

        Assert.NotNull(actual);
        Assert.Null(actual.Snapshot);
        Assert.Equal([RenoDX.Branch.Direct], actual.Branches);
    }

    [Fact]
    public void Resolve_SnapshotFileListUnavailable_ReturnsSnapshot()
    {
        var snapshotWithoutFileList = CreateTag(7, RenoDX.Branch.Snapshot);

        var actual = RenoDXAddonFileResolver.Resolve(AddonFilename, null, snapshotWithoutFileList,
            []);

        Assert.NotNull(actual);
        Assert.Same(snapshotWithoutFileList, actual.Snapshot);
        Assert.Equal([RenoDX.Branch.Snapshot], actual.Branches);
    }

    [Fact]
    public void Resolve_FileOnNoListAndNoDirectUrl_ReturnsNothing()
    {
        var snapshot = CreateTag(7, RenoDX.Branch.Snapshot, "renodx-other.addon64");
        var nightly = CreateTag(6, RenoDX.Branch.Nightly, "renodx-other.addon64");

        var actual = RenoDXAddonFileResolver.Resolve(AddonFilename, null, snapshot,
            [nightly]);

        Assert.Null(actual);
    }

    private static RenoDXTagInfo CreateTag(int day, RenoDX.Branch branch, params string[] addonFilenames) =>
        new(new DateOnly(2026, 10, day), branch, new Uri("https://restalltests.com/releases/"),
            addonFilenames.ToFrozenSet(StringComparer.OrdinalIgnoreCase));
}
