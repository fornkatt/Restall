// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Helpers;

namespace Restall.Application.Tests.HelperTests;

public class RenoDXModMatcherTests
{
    [Fact]
    public void Match_SameNameInGameModList_ReturnsExactGameMod()
    {
        var gameMod = CreateGameMod("Silent Hill: Townfall");
        var database = new RenoDXModDatabase([gameMod], [], []);

        var actual = RenoDXModMatcher.Match("SILENT HILL: Townfall", database);

        Assert.Equal(RenoDXModMatch.MatchKind.Exact, actual.Kind);
        Assert.Equal(gameMod, actual.GameMod);
    }

    [Fact]
    public void Match_AccentedGameName_ReturnsExactGameModWithoutAccents()
    {
        var olderGameMod = CreateGameMod("God of War (2018)");
        var gameMod = CreateGameMod("God of War Ragnarok");
        var database = new RenoDXModDatabase([olderGameMod, gameMod], [], []);

        var actual = RenoDXModMatcher.Match("God of War Ragnarök", database);

        Assert.Equal(RenoDXModMatch.MatchKind.Exact, actual.Kind);
        Assert.Equal(gameMod, actual.GameMod);
    }

    [Theory]
    [InlineData("Middle-earth™: Shadow of War™", "Middle Earth: Shadow of War")]
    [InlineData("Watch_Dogs", "Watch Dogs")]
    public void Match_InWordSeparatorInGameName_ReturnsExactGameModWithSpace(string gameName, string modName)
    {
        var gameMod = CreateGameMod(modName);
        var database = new RenoDXModDatabase([gameMod], [], []);

        var actual = RenoDXModMatcher.Match(gameName, database);

        Assert.Equal(RenoDXModMatch.MatchKind.Exact, actual.Kind);
        Assert.Equal(gameMod, actual.GameMod);
    }

    [Fact]
    public void Match_SameNameInGameModAndUnrealLists_ReturnsGameMod()
    {
        var gameMod = CreateGameMod("S.T.A.L.K.E.R. 2: Heart of Chornobyl");
        var unrealGenericMod = CreateUnrealGenericMod("S.T.A.L.K.E.R. 2: Heart of Chornobyl");
        var database = new RenoDXModDatabase([gameMod], [unrealGenericMod], []);

        var actual = RenoDXModMatcher.Match("S.T.A.L.K.E.R. 2: Heart of Chornobyl", database);

        Assert.Equal(RenoDXModMatch.MatchKind.Exact, actual.Kind);
        Assert.Equal(gameMod, actual.GameMod);
        Assert.Null(actual.UnrealGenericMod);
    }

    [Fact]
    public void Match_FuzzyGameModAndExactUnityGenericMod_ReturnsUnityGenericMod()
    {
        var gameMod = CreateGameMod("Fuzzy Game Mod");
        var unityGenericMod = CreateUnityGenericMod("Fuzzy Game");
        var database = new RenoDXModDatabase([gameMod], [], [unityGenericMod]);

        var actual = RenoDXModMatcher.Match("Fuzzy Game", database);

        Assert.Equal(RenoDXModMatch.MatchKind.Exact, actual.Kind);
        Assert.Equal(unityGenericMod, actual.UnityGenericMod);
        Assert.Null(actual.GameMod);
    }

    [Fact]
    public void Match_SeveralFuzzyCandidates_ReturnsCandidateWithMostSharedWords()
    {
        var fewerSharedWordsGameMod = CreateGameMod("Tomb Raider (2013) - Definitive Edition");
        var gameMod = CreateGameMod("Shadow of the Tomb Raider");
        var database = new RenoDXModDatabase([fewerSharedWordsGameMod, gameMod], [],
            []);

        var actual = RenoDXModMatcher.Match("Shadow of the Tomb Raider: Definitive Edition",
            database);

        Assert.Equal(RenoDXModMatch.MatchKind.Fuzzy, actual.Kind);
        Assert.Equal(gameMod, actual.GameMod);
    }

    [Fact]
    public void Match_FuzzyCandidatesWithEqualSharedWords_ReturnsTieWithTiedNames()
    {
        var gameMod = CreateGameMod("Fuzzy Game Mod");
        var otherGameMod = CreateGameMod("Fuzzy Game Dom");
        var database = new RenoDXModDatabase([gameMod, otherGameMod], [], []);

        var actual = RenoDXModMatcher.Match("Fuzzy Game", database);

        Assert.Equal(RenoDXModMatch.MatchKind.Tie, actual.Kind);
        Assert.Equal(["Fuzzy Game Mod", "Fuzzy Game Dom"], actual.TiedNames);
        Assert.Null(actual.GameMod);
    }

    [Fact]
    public void Match_NoSimilarModName_ReturnsNothing()
    {
        var gameMod = CreateGameMod("Alan Wake II");
        var unrealGenericMod = CreateUnrealGenericMod("RoboCop: Rogue City");
        var unityGenericMod = CreateUnityGenericMod("Tainted Grail: The Fall of Avalon");
        var database = new RenoDXModDatabase([gameMod], [unrealGenericMod],
            [unityGenericMod]);

        var actual = RenoDXModMatcher.Match("Test Game", database);

        Assert.Same(RenoDXModMatch.None, actual);
    }

    [Fact]
    public void Match_ExpansionNameWithSubtitle_ReturnsNothing()
    {
        var database = new RenoDXModDatabase([CreateGameMod("RoboCop: Rogue City")],
            [], []);

        var actual = RenoDXModMatcher.Match("RoboCop: Rogue City - Unfinished Business",
            database);

        Assert.Same(RenoDXModMatch.None, actual);
    }

    [Fact]
    public void Match_GameWithSimilarName_ReturnsNoMatch()
    {
        var database = new RenoDXModDatabase([],
            [CreateUnrealGenericMod("Assetto Corsa Rally")], []);

        var actual = RenoDXModMatcher.Match("Assetto Corsa", database);

        Assert.Same(RenoDXModMatch.None, actual);
    }

    private static RenoDXGameMod CreateGameMod(string name) =>
        new(name, RenoDXModStatus.Done, null, null, null, null, null,
            null, null);

    private static RenoDXUnrealGenericMod CreateUnrealGenericMod(string name) =>
        new(name, RenoDXModStatus.Done, RenoDXUnrealGenericMod.UnrealModMethod.Native, null, null);

    private static RenoDXUnityGenericMod CreateUnityGenericMod(string name) =>
        new(name, RenoDXModStatus.Done, null, null);
}
