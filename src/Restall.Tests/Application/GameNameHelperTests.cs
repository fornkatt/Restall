using Restall.Application.Helpers;

namespace Restall.Tests.Application;

public sealed class GameNameHelperTests
{
    // Verifies that normalization trims, lowercases, and removes punctuation from game names.
    [Theory]
    [InlineData("  Cyberpunk 2077: Ultimate Edition!  ", "cyberpunk 2077 ultimate edition")]
    [InlineData("Baldur's Gate 3", "baldurs gate 3")]
    [InlineData("FINAL-FANTASY VII", "finalfantasy vii")]
    public void NormalizeName_RemovesPunctuationAndNormalizesCasing(string input, string expected)
    {
        var result = GameNameHelper.NormalizeName(input);

        Assert.Equal(expected, result);
    }

    // Verifies that blank game names normalize to an empty string.
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void NormalizeName_WhenInputIsBlank_ReturnsEmptyString(string? input)
    {
        var result = GameNameHelper.NormalizeName(input!);

        Assert.Equal(string.Empty, result);
    }

    // Verifies that common edition suffixes are stripped from game names.
    [Theory]
    [InlineData("Control Deluxe Edition", "Control")]
    [InlineData("The Witcher 3 GOTY", "The Witcher 3")]
    [InlineData("Elden Ring Game of the Year", "Elden Ring")]
    [InlineData("Alan Wake Remastered", "Alan Wake")]
    public void StripEditionSuffix_RemovesKnownEditionSuffixes(string input, string expected)
    {
        var result = GameNameHelper.StripEditionSuffix(input);

        Assert.Equal(expected, result);
    }

    // Verifies that names without edition suffixes are not changed.
    [Theory]
    [InlineData("Hades")]
    [InlineData("Control Bureau")]
    public void StripEditionSuffix_WhenNoMatchingSuffixExists_ReturnsOriginalName(string input)
    {
        var result = GameNameHelper.StripEditionSuffix(input);

        Assert.Equal(input, result);
    }

    // Verifies that fuzzy matching accepts close name variants with shared word coverage.
    [Theory]
    [InlineData("the witcher 3 wild hunt", "witcher 3")]
    [InlineData("resident evil village", "resident evil")]
    [InlineData("final fantasy vii remake", "final fantasy vii")]
    public void FuzzyNameMatch_WhenNamesShareEnoughWords_ReturnsTrue(string a, string b)
    {
        var result = GameNameHelper.FuzzyNameMatch(a, b);

        Assert.True(result);
    }

    // Verifies that fuzzy matching rejects unrelated or too-weak name matches.
    [Theory]
    [InlineData("hades", "doom eternal")]
    [InlineData("star", "starfield")]
    [InlineData("resident evil", "village")]
    public void FuzzyNameMatch_WhenNamesAreUnrelatedOrTooWeak_ReturnsFalse(string a, string b)
    {
        var result = GameNameHelper.FuzzyNameMatch(a, b);

        Assert.False(result);
    }
}
