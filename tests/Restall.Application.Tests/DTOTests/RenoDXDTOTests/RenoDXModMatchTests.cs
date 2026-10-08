// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Common.Enums;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

public class RenoDXModMatchTests
{
    [Fact]
    public void ModMatchNone_NoDatabaseEntry_ReturnsNothing()
    {
        var noMatch = RenoDXModMatch.None;

        Assert.Null(noMatch.GameMod);
        Assert.Null(noMatch.UnrealGenericMod);
        Assert.Null(noMatch.UnityGenericMod);
        Assert.Equal(RenoDXModMatch.MatchKind.None, noMatch.Kind);
        Assert.Empty(noMatch.TiedNames);
    }

    [Fact]
    public void GetAddonFilename_GameModMatch_ReturnsGameModFileForArchitecture()
    {
        var gameMod = new RenoDXGameMod("Test Game", RenoDXModStatus.Done, null,
            "https://restalltests.com/renodx-game.addon64",
            "https://restalltests.com/renodx-game.addon32",
            null, null, null, null);
        var match = new RenoDXModMatch(gameMod, null, null,
            RenoDXModMatch.MatchKind.Exact, []);

        var actual = match.GetAddonFilename(Architecture.X32);

        Assert.Equal("renodx-game.addon32", actual);
    }

    [Fact]
    public void GetAddonFilename_UnrealGenericMatch_ReturnsUnrealExtendedFile()
    {
        var unrealGenericMod = new RenoDXUnrealGenericMod("Test Game", RenoDXModStatus.Done,
            RenoDXUnrealGenericMod.UnrealModMethod.Native, null, null);
        var match = new RenoDXModMatch(null, unrealGenericMod, null,
            RenoDXModMatch.MatchKind.Exact, []);

        var actual = match.GetAddonFilename(Architecture.X64);

        Assert.Equal("renodx-ue-extended.addon64", actual);
    }

    [Fact]
    public void GetAddonFilename_UnityGenericMatch_ReturnsUnityGenericFileForArchitecture()
    {
        var unityGenericMod = new RenoDXUnityGenericMod("Test Game", RenoDXModStatus.Done, null,
            null);
        var match = new RenoDXModMatch(null, null, unityGenericMod,
            RenoDXModMatch.MatchKind.Exact,
            []);

        var actual = match.GetAddonFilename(Architecture.X32);

        Assert.Equal("renodx-unityengine.addon32", actual);
    }

    [Fact]
    public void GetAddonFilename_NoMatch_ReturnsNull()
    {
        Assert.Null(RenoDXModMatch.None.GetAddonFilename(Architecture.X64));
    }

    [Fact]
    public void ModName_UnityGenericMatch_ReturnsUnityGenericModName()
    {
        var unityGenericMod = new RenoDXUnityGenericMod("Unity Game", RenoDXModStatus.Done, null,
            null);
        var actual = new RenoDXModMatch(null, null, unityGenericMod,
            RenoDXModMatch.MatchKind.Fuzzy, []);

        Assert.Equal("Unity Game", actual.ModName);
    }

    [Fact]
    public void ModStatus_UnrealGenericMatch_ReturnsUnrealGenericModStatus()
    {
        var unrealGenericMod = new RenoDXUnrealGenericMod("Unreal Game", RenoDXModStatus.Wip,
            RenoDXUnrealGenericMod.UnrealModMethod.Upgrade, null, null);
        var actual = new RenoDXModMatch(null, unrealGenericMod, null,
            RenoDXModMatch.MatchKind.Exact, []);

        Assert.Equal("Unreal Game", actual.ModName);
    }
}
