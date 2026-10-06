// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

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
    public void RemoveLatinAccents_KoreanAndJapaneseLetters_ReturnsNameUnchanged(string letters, string expected)
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
        const string game = "RoboCop: Rogue City - Unfinished Business";

        var actual = GameNameHelper.SplitInWordSeparators(game);

        Assert.Equal(game, actual);
    }

    [Fact]
    public void CountSharedWords_NamesWithCommonWords_ReturnsSharedWordCount()
    {
        var actual = GameNameHelper.CountSharedWords("Shadow of the Tomb Raider: Definitive Edition",
            "Shadow of the Tomb Raider");

        Assert.Equal(5, actual);
    }

    [Fact]
    public void CountSharedWords_NoCommonWords_ReturnsZero()
    {
        var actual = GameNameHelper.CountSharedWords("SILENT HILL: Townfall", "CONTROL Resonant");

        Assert.Equal(0, actual);
    }
}
