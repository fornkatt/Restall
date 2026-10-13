// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Helpers;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;
using System.Collections.Frozen;

namespace Restall.Application.Tests.HelperTests;

public class RenoDXEntryBuilderTests
{
    private const string AddonFilename = "renodx-game.addon64";

    private static readonly RenoDXGameMod s_gameMod =
        new("Test Game", RenoDXModStatus.Done, "Test Author",
            "https://restalltests.com/mods/renodx-game.addon64", null,
            "https://www.nexusmods.com/testgame/mods/1", "https://discord.com/channels/1/2",
            "https://github.com/clshortfuse/renodx/discussions/1", "Test notes");

    private static readonly RenoDXTagInfo s_snapshot = new(new DateOnly(2026, 10, 7), new Uri("https://restalltests.com/releases/"),
        new[] { AddonFilename }.ToFrozenSet(StringComparer.OrdinalIgnoreCase), []);

    [Fact]
    public void Build_GameModMatch_ReturnsGameModInformationAndLinksInOrder()
    {
        var match = new RenoDXModMatch(s_gameMod, null, null,
            RenoDXModMatch.MatchKind.Exact, []);

        var actual = BuildRenoDXEntry(null, Game.Engine.Unknown, match);

        Assert.Equal(RenoDXTexts.GetListedAsText("Test Game"), actual.HeadingText);
        Assert.Equal("Test Author", actual.Author);
        Assert.Equal("Test notes", actual.DatabaseNotes);
        Assert.Equal(RenoDXTexts.GetStatusText(RenoDXModStatus.Done), actual.StatusText);
        Assert.Equal([new Uri(s_gameMod.NexusUrl!), new Uri(s_gameMod.DiscordUrl!), new Uri(s_gameMod.DiscussionUrl!)],
            actual.Links.Select(l => l.Url));
    }

    [Fact]
    public void Build_UnrealGenericMatch_ReturnsMethodTextUpgradeTokens()
    {
        var unrealGenericMod = new RenoDXUnrealGenericMod("Test Unreal Game", RenoDXModStatus.Wip,
            RenoDXUnrealGenericMod.UnrealModMethod.Upgrade, "`B8G8R8A8_TYPELESS` `Output Size`",
            "Test comments");
        var match = new RenoDXModMatch(null, unrealGenericMod, null,
            RenoDXModMatch.MatchKind.Exact, []);
        var actual = BuildRenoDXEntry(null, Game.Engine.Unreal, match);

        Assert.Equal(RenoDXTexts.GetUnrealMethodText(RenoDXUnrealGenericMod.UnrealModMethod.Upgrade),
            actual.UnrealMethodText);
        Assert.Equal(["B8G8R8A8_TYPELESS", "Output Size"], actual.Upgrades);
        Assert.Equal("Test comments", actual.DatabaseNotes);
    }

    [Fact]
    public void Build_UnrealGenericMatch_ReturnsNoAuthor()
    {
        var unrealGenericMod = new RenoDXUnrealGenericMod("Test Unreal Game", RenoDXModStatus.Done,
            RenoDXUnrealGenericMod.UnrealModMethod.Native, null, null);
        var match = new RenoDXModMatch(null, unrealGenericMod, null,
            RenoDXModMatch.MatchKind.Exact, []);

        var actual = BuildRenoDXEntry(null, Game.Engine.Unreal, match);

        Assert.NotNull(actual.GenericAddonInfo);
        Assert.Null(actual.Author);
    }

    [Fact]
    public void Build_UnrealGameWithoutMatch_ReturnsNoticeText()
    {
        var actual = BuildRenoDXEntry(null, Game.Engine.Unreal, RenoDXModMatch.None);

        Assert.Equal([RenoDXTexts.GetNoticeText(RenoDXAvailability.Notice.EngineFallback)], actual.Notices);
    }

    [Theory]
    [InlineData(Game.Engine.Unreal)]
    [InlineData(Game.Engine.Unity)]
    public void Build_GenericGameWithoutMatch_ReturnsGenericAddonHeadingText(Game.Engine engine)
    {
        var actual = BuildRenoDXEntry(null, engine, RenoDXModMatch.None);

        Assert.NotNull(actual.GenericAddonInfo);
        Assert.Equal(actual.GenericAddonInfo.HeadingText, actual.HeadingText);
    }

    [Fact]
    public void Build_WipGameModMatch_ReturnsWorkInProgress()
    {
        var match = new RenoDXModMatch(s_gameMod with { Status = RenoDXModStatus.Wip }, null,
            null, RenoDXModMatch.MatchKind.Exact, []);

        var actual = BuildRenoDXEntry(null, Game.Engine.Unreal, match);

        Assert.True(actual.IsWorkInProgress);
    }

    [Fact]
    public void Build_DoneGameModMatch_ReturnsIsDone()
    {
        var match = new RenoDXModMatch(s_gameMod with { Status = RenoDXModStatus.Done }, null,
            null, RenoDXModMatch.MatchKind.Exact, []);

        var actual = BuildRenoDXEntry(null, Game.Engine.Unreal, match);

        Assert.True(actual.IsDone);
    }

    [Fact]
    public void Build_InstalledRenoDXOfOtherBitness_ReturnsBranchDataForNothingInstalled()
    {
        var installedRenoDX = new RenoDX
        {
            OriginalName = "renodx-game.addon64",
            Arch = Architecture.X32,
            Version = "20261001"
        };
        var nightly = new RenoDXTagInfo(new DateOnly(2026, 10, 1),
            new Uri("https://restalltests.com/releases/"),
            new[] { AddonFilename }.ToFrozenSet(StringComparer.OrdinalIgnoreCase), []);
        var match = new RenoDXModMatch(s_gameMod, null, null,
            RenoDXModMatch.MatchKind.Exact, []);

        var actual = RenoDXEntryBuilder.Build(installedRenoDX, Game.Engine.Unknown, Architecture.X64,
            match, true, s_snapshot, [nightly]);

        Assert.False(actual.BranchVersions[RenoDX.Branch.Snapshot].UpdateAvailable);
        Assert.False(Assert.Single(actual.BranchReleaseNotes[RenoDX.Branch.Nightly]).IsInstalled);
    }

    private static RenoDXEntry BuildRenoDXEntry(RenoDX? installedRenoDX, Game.Engine engine, RenoDXModMatch match) =>
        RenoDXEntryBuilder.Build(installedRenoDX, engine, Architecture.X64, match, true,
            s_snapshot, []);
}
