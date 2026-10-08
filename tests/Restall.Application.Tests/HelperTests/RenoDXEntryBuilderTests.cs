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

    private static readonly RenoDXTagInfo s_snapshot = new(new DateOnly(2026, 10, 7),
        RenoDX.Branch.Snapshot, new Uri("https://restalltests.com/releases/"),
        new[] { AddonFilename }.ToFrozenSet(StringComparer.OrdinalIgnoreCase));

    [Fact]
    public void Build_GameModMatch_ReturnsGameModInformationAndLinksInOrder()
    {
        var match = new RenoDXModMatch(s_gameMod, null, null,
            RenoDXModMatch.MatchKind.Exact, []);

        var actual = BuildRenoDXEntry(null, Game.Engine.Unknown, match);

        Assert.Equal("Test Game", actual.ListedName);
        Assert.Equal("Test Author", actual.Author);
        Assert.Equal("Test notes", actual.DatabaseNotes);
        Assert.Equal(RenoDXTexts.GetStatusText(RenoDXModStatus.Done), actual.StatusText);
        Assert.Equal(["Nexus Mods", "Discord", "GitHub discussion"], actual.Links.Select(l => l.Label));
    }

    [Fact]
    public void Build_UnrealGenericMatch_ReturnsMethodTextUpgradeTokensAndGenericAuthor()
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
        Assert.Equal("Marat", actual.Author);
    }

    [Fact]
    public void Build_UnrealGameWithoutMatch_ReturnsNoticeText()
    {
        var actual = BuildRenoDXEntry(null, Game.Engine.Unreal, RenoDXModMatch.None);

        Assert.Equal([RenoDXTexts.GetNoticeText(RenoDXAvailability.Notice.EngineFallback)], actual.Notices);
    }

    private static RenoDXEntry BuildRenoDXEntry(RenoDX? installedRenoDX, Game.Engine engine, RenoDXModMatch match) =>
        RenoDXEntryBuilder.Build(installedRenoDX, engine, Architecture.X64, match, true,
            s_snapshot, []);
}
