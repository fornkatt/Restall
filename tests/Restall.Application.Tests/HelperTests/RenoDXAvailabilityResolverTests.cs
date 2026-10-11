// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Helpers;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;
using System.Collections.Frozen;

namespace Restall.Application.Tests.HelperTests;

public class RenoDXAvailabilityResolverTests
{
    private const string AddonFilename = "renodx-game.addon64";
    private const string NexusUrl = "https://www.nexusmods.com/testgame/mods/1";

    private static readonly RenoDXGameMod s_gameMod = new("Test Game", RenoDXModStatus.Done, null,
        "https://restalltests.com/mods/renodx-game.addon64",
        null, null, null, null, null);

    private static readonly RenoDXTagInfo s_snapshotWithFile = CreateSnapshot(AddonFilename);

    [Fact]
    public void Resolve_GameModWithSnapshot_ReturnsGameModFile()
    {
        var actual = RenoDXAvailabilityResolver.Resolve(null, Game.Engine.Unknown,
            Architecture.X64, CreateMatch(s_gameMod), true, s_snapshotWithFile, []);

        Assert.True(actual.IsSupported);
        Assert.NotNull(actual.DownloadOptions);
        Assert.Equal(AddonFilename, actual.DownloadOptions.Filename);
        Assert.Equal(new Uri("https://restalltests.com/mods/renodx-game.addon64"), actual.DownloadOptions.DirectUrl);
        Assert.Empty(actual.Notices);
    }

    [Fact]
    public void Resolve_InstalledRenoDXWithRecommendedArchitecture_ReturnsInstalledFile()
    {
        const string userPickedAddonFilename = "renodx-userpicked.addon64";
        var installedRenoDX = new RenoDX { Arch = Architecture.X64, OriginalName = userPickedAddonFilename };
        var snapshot = CreateSnapshot(AddonFilename, userPickedAddonFilename);

        var actual = RenoDXAvailabilityResolver.Resolve(installedRenoDX, Game.Engine.Unknown,
            Architecture.X64, CreateMatch(s_gameMod), true, snapshot, []);

        Assert.NotNull(actual.DownloadOptions);
        Assert.Equal("renodx-userpicked.addon64", actual.DownloadOptions.Filename);
        Assert.True(actual.DownloadOptions.IsInstalledFile);
        Assert.Empty(actual.Notices);
    }

    [Fact]
    public void Resolve_InstalledRenoDXWithOtherArchitecture_ReturnsMismatchNotice()
    {
        var installedRenoDX = new RenoDX { Arch = Architecture.X32, OriginalName = "renodx-game.addon32" };

        var actual = RenoDXAvailabilityResolver.Resolve(installedRenoDX, Game.Engine.Unknown,
            Architecture.X64, CreateMatch(s_gameMod), true, s_snapshotWithFile, []);

        Assert.NotNull(actual.DownloadOptions);
        Assert.Equal(AddonFilename, actual.DownloadOptions.Filename);
        Assert.False(actual.DownloadOptions.IsInstalledFile);
        Assert.Equal([RenoDXAvailability.Notice.InstalledArchitectureMismatch], actual.Notices);
    }

    [Fact]
    public void Resolve_GameModWithoutBuildForArchitecture_ReturnsNoBuildNotice()
    {
        var actual = RenoDXAvailabilityResolver.Resolve(null, Game.Engine.Unknown,
            Architecture.X32, CreateMatch(s_gameMod), true, s_snapshotWithFile, []);

        Assert.True(actual.IsSupported);
        Assert.Null(actual.DownloadOptions);
        Assert.Null(actual.ManualSource);
        Assert.Equal([RenoDXAvailability.Notice.NoBuildForArchitecture], actual.Notices);
    }

    [Fact]
    public void Resolve_GameModWithNexusAndDiscordModPage_ReturnsNexusManualSource()
    {
        var gameMod = s_gameMod with
        {
            SnapshotUrl = null, NexusUrl = NexusUrl, DiscordUrl = "https://discord.com/channels/1/2"
        };

        var actual = RenoDXAvailabilityResolver.Resolve(null, Game.Engine.Unknown,
            Architecture.X64, CreateMatch(gameMod), true, s_snapshotWithFile, []);

        Assert.Null(actual.DownloadOptions);
        Assert.NotNull(actual.ManualSource);
        Assert.Equal("Nexus Mods", actual.ManualSource.Label);
        Assert.Empty(actual.Notices);
    }

    [Fact]
    public void Resolve_GameModWithoutAnySource_ReturnsNoDownloadListedNotice()
    {
        var gameMod = s_gameMod with { SnapshotUrl = null };

        var actual = RenoDXAvailabilityResolver.Resolve(null, Game.Engine.Unknown,
            Architecture.X64, CreateMatch(gameMod), true, s_snapshotWithFile, []);

        Assert.True(actual.IsSupported);
        Assert.Null(actual.DownloadOptions);
        Assert.Null(actual.ManualSource);
        Assert.Equal([RenoDXAvailability.Notice.NoDownloadListed], actual.Notices);
    }

    [Fact]
    public void Resolve_UnrealGameWithoutMatch_ReturnsEngineFallback()
    {
        var actual = RenoDXAvailabilityResolver.Resolve(null, Game.Engine.Unreal,
            Architecture.X64, RenoDXModMatch.None, true, s_snapshotWithFile, []);

        Assert.True(actual.IsSupported);
        Assert.NotNull(actual.DownloadOptions);
        Assert.NotNull(actual.GenericAddonInfo);
        Assert.Equal([RenoDXAvailability.Notice.EngineFallback], actual.Notices);
    }

    [Fact]
    public void Resolve_UnityGameWithGameModListNotLoaded_ReturnsNotSupported()
    {
        var actual = RenoDXAvailabilityResolver.Resolve(null, Game.Engine.Unity,
            Architecture.X64, RenoDXModMatch.None, false, s_snapshotWithFile, []);

        Assert.False(actual.IsSupported);
        Assert.Null(actual.DownloadOptions);
        Assert.Empty(actual.Notices);
    }

    [Fact]
    public void Resolve_TieOnUnrealGame_ReturnsNotSupportedWithAmbiguousMatchNotice()
    {
        var tie = new RenoDXModMatch(null, null, null, RenoDXModMatch.MatchKind.Tie,
            ["Test Game Part One", "Test Game Part Two"]);

        var actual = RenoDXAvailabilityResolver.Resolve(null, Game.Engine.Unreal,
            Architecture.X64, tie, true, s_snapshotWithFile, []);

        Assert.False(actual.IsSupported);
        Assert.Null(actual.DownloadOptions);
        Assert.Equal([RenoDXAvailability.Notice.AmbiguousMatch], actual.Notices);
    }

    [Fact]
    public void Resolve_GenericAddonInstalledWithGameMod_ReturnsGameSpecificModNotice()
    {
        var installedRenoDX = new RenoDX { Arch = Architecture.X64, OriginalName = "renodx-ue-extended.addon64" };

        var actual = RenoDXAvailabilityResolver.Resolve(installedRenoDX, Game.Engine.Unreal,
            Architecture.X64, CreateMatch(s_gameMod), true, s_snapshotWithFile, []);

        Assert.NotNull(actual.DownloadOptions);
        Assert.Equal([RenoDXAvailability.Notice.GameSpecificModAvailable], actual.Notices);
    }

    [Fact]
    public void Resolve_SnapshotFileListUnavailable_ReturnsSnapshotFileUnconfirmedNotice()
    {
        var snapshotWithoutFileList = CreateSnapshot();

        var actual = RenoDXAvailabilityResolver.Resolve(null, Game.Engine.Unknown,
            Architecture.X64, CreateMatch(s_gameMod), true, snapshotWithoutFileList, []);

        Assert.NotNull(actual.DownloadOptions);
        Assert.Equal([RenoDXAvailability.Notice.SnapshotFileUnconfirmed], actual.Notices);
    }

    [Fact]
    public void Resolve_GameModOffersGenericAddon_ReturnsNoGameSpecificModNoticeAndGenericAddonInfo()
    {
        var gameModWithGenericAddon = s_gameMod with
        {
            SnapshotUrl = "https://restalltests.com/releases/renodx-ue-extended.addon64"
        };

        var actual = RenoDXAvailabilityResolver.Resolve(null, Game.Engine.Unreal,
            Architecture.X64, CreateMatch(gameModWithGenericAddon), true,
            s_snapshotWithFile, []);

        Assert.Null(actual.GenericAddonInfo);
        Assert.Empty(actual.Notices);
    }

    private static RenoDXModMatch CreateMatch(RenoDXGameMod gameMod) =>
        new(gameMod, null, null, RenoDXModMatch.MatchKind.Exact, []);

    private static RenoDXTagInfo CreateSnapshot(params string[] addonFilenames) =>
        new(new DateOnly(2026, 10, 7), RenoDX.Branch.Snapshot,
            new Uri("https://restalltests.com/releases/"),
            addonFilenames.ToFrozenSet(StringComparer.OrdinalIgnoreCase));
}
