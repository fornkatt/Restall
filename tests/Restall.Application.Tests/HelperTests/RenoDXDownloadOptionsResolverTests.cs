// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Helpers;
using Restall.Domain.Entities;
using System.Collections.Frozen;

namespace Restall.Application.Tests.HelperTests;

public class RenoDXDownloadOptionsResolverTests
{
    private const string AddonFilename = "renodx-game.addon64";

    private static readonly Uri s_directUrl = new("https://restalltests.com/direct/renodx-game.addon64");

    [Fact]
    public void Resolve_FileOnSnapshotAndNightlyWithDirectUrl_ReturnsAllBranches()
    {
        var snapshot = CreateTag(7, AddonFilename);
        var nightly = CreateTag(6, AddonFilename);

        var actual = RenoDXDownloadOptionsResolver.Resolve(AddonFilename, s_directUrl, snapshot,
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
        var nightlyWithFile = CreateTag(7, AddonFilename);
        var nightlyWithoutFile = CreateTag(6,
            "renodx-other.addon64");

        var actual = RenoDXDownloadOptionsResolver.Resolve(AddonFilename, null, null,
            [nightlyWithFile, nightlyWithoutFile]);

        Assert.NotNull(actual);
        Assert.Same(nightlyWithFile, Assert.Single(actual.Nightlies));
        Assert.Equal([RenoDX.Branch.Nightly], actual.Branches);
    }

    [Fact]
    public void Resolve_FileMissingFromSnapshotList_ReturnsNoSnapshot()
    {
        var snapshot = CreateTag(7, "renodx-other.addon64");

        var actual = RenoDXDownloadOptionsResolver.Resolve(AddonFilename, s_directUrl, snapshot, []);

        Assert.NotNull(actual);
        Assert.Null(actual.Snapshot);
        Assert.Equal([RenoDX.Branch.Direct], actual.Branches);
    }

    [Fact]
    public void Resolve_SnapshotFileListUnavailable_ReturnsSnapshot()
    {
        var snapshotWithoutFileList = CreateTag(7);

        var actual = RenoDXDownloadOptionsResolver.Resolve(AddonFilename, null, snapshotWithoutFileList,
            []);

        Assert.NotNull(actual);
        Assert.Same(snapshotWithoutFileList, actual.Snapshot);
        Assert.Equal([RenoDX.Branch.Snapshot], actual.Branches);
    }

    [Fact]
    public void Resolve_FileOnNoListAndNoDirectUrl_ReturnsNothing()
    {
        var snapshot = CreateTag(7, "renodx-other.addon64");
        var nightly = CreateTag(6, "renodx-other.addon64");

        var actual = RenoDXDownloadOptionsResolver.Resolve(AddonFilename, null, snapshot,
            [nightly]);

        Assert.Null(actual);
    }

    [Fact]
    public void Resolve_GenericAddonWithSnapshotFileListUnavailable_ReturnsNoSnapshot()
    {
        const string genericAddonFilename = "renodx-ue-extended.addon64";
        var snapshotWithoutFileList = CreateTag(7);

        var actual = RenoDXDownloadOptionsResolver.Resolve(genericAddonFilename, s_directUrl,
            snapshotWithoutFileList, []);

        Assert.NotNull(actual);
        Assert.Null(actual.Snapshot);
        Assert.Equal([RenoDX.Branch.Direct], actual.Branches);
    }

    private static RenoDXTagInfo CreateTag(int day, params string[] addonFilenames) =>
        new(new DateOnly(2026, 10, day), new Uri("https://restalltests.com/releases/"),
            addonFilenames.ToFrozenSet(StringComparer.OrdinalIgnoreCase), []);
}
