// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.Helpers;

namespace Restall.Application.Tests.HelperTests;

public class GameNameHelperTests
{
    [Theory]
    [InlineData("God of War Ragnarök", "God of War Ragnarok")]
    [InlineData("Pokémon", "Pokemon")]
    [InlineData("ABZÛ", "ABZU")]
    public void RemoveLatinAccents_AccentedLetter_ReturnsBaseLetter(string name, string expected)
    {
        var actual = GameNameHelper.RemoveLatinAccents(name);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RemoveLatinAccents_AccentStoredAsSeparateChar_ReturnsBaseLetter()
    {
        var actual = GameNameHelper.RemoveLatinAccents("Ragnaro\u0308k");

        Assert.Equal("Ragnarok", actual);
    }

    [Fact]
    public void RemoveLatinAccents_LetterWithoutBaseAccentLetter_ReturnsNameUnchanged()
    {
        var actual = GameNameHelper.RemoveLatinAccents("SNØ");

        Assert.Equal("SNØ", actual);
    }

    [Theory]
    [InlineData("きょうふ", "きょうふ")]
    [InlineData("恐怖", "恐怖")]
    [InlineData("무서움", "무서움")]
    [InlineData("サイコブレイク", "サイコブレイク")]
    [InlineData("パン", "パン")]
    public void RemoveLatinAccents_AsianLanguageLetters_ReturnsNameUnchanged(string letters, string expected)
    {
        var actual = GameNameHelper.RemoveLatinAccents(letters);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("Middle-earth", "Middle earth")]
    [InlineData("Watch_Dogs", "Watch Dogs")]
    public void SplitInWordSeparators_SeparatorBetweenLetters_ReturnsSpace(string name, string expected)
    {
        var actual = GameNameHelper.SplitInWordSeparators(name);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SplitInWordSeparators_HyphenBetweenSpaces_ReturnsNameUnchanged()
    {
        const string gameName = "RoboCop: Rogue City - Unfinished Business";

        var actual = GameNameHelper.SplitInWordSeparators(gameName);

        Assert.Equal(gameName, actual);
    }

    [Fact]
    public void IsLikelySameGame_ExpansionWithSubtitle_ReturnsFalse()
    {
        var actual = GameNameHelper.IsLikelySameGame(
            "RoboCop: Rogue City - Unfinished Business",
            "RoboCop: Rogue City");

        Assert.False(actual);
    }

    [Fact]
    public void IsLikelySameGame_PreparedExpansionWithSubtitle_ReturnsFalse()
    {
        var actual = GameNameHelper.IsLikelySameGame(
            PreparedName.Create("RoboCop: Rogue City - Unfinished Business"),
            PreparedName.Create("RoboCop: Rogue City"));

        Assert.False(actual);
    }

    [Fact]
    public void CountSharedWords_NamesWithCommonWords_ReturnsSharedWordCount()
    {
        var actual = GameNameHelper.CountSharedWords(
            PreparedName.Create("Shadow of the Tomb Raider: Definitive Edition"),
            PreparedName.Create("Shadow of the Tomb Raider"));

        Assert.Equal(5, actual);
    }

    [Fact]
    public void CountSharedWords_NoCommonWords_ReturnsZero()
    {
        var actual = GameNameHelper.CountSharedWords(
            PreparedName.Create("SILENT HILL: Townfall"),
            PreparedName.Create("CONTROL Resonant"));

        Assert.Equal(0, actual);
    }
}
