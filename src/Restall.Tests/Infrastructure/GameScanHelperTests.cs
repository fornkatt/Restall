using Restall.Infrastructure.Helpers;

namespace Restall.Tests.Infrastructure;

public sealed class GameScanHelperTests
{
    // Verifies that null and empty paths are treated as missing values.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NormalizePath_WhenPathIsNullOrEmpty_ReturnsNull(string? path)
    {
        var result = GameScanHelper.NormalizePath(path);

        Assert.Null(result);
    }

    // Verifies that path separators, whitespace and trailing separators are normalized.
    [Fact]
    public void NormalizePath_WhenPathHasAltSeparatorsAndTrailingSlash_ReturnsNormalizedPath()
    {
        var result = GameScanHelper.NormalizePath("  C:/Games/Alpha/  ");

        Assert.Equal($"C:{Path.DirectorySeparatorChar}Games{Path.DirectorySeparatorChar}Alpha", result);
    }

    // Verifies that VDF values are extracted case-insensitively.
    [Fact]
    public void ExtractVdfValue_WhenKeyExists_ReturnsValueIgnoringKeyCase()
    {
        const string vdf = "\"AppState\"\n{\n  \"name\" \"Alpha Game\"\n  \"installdir\" \"Alpha Folder\"\n}";

        Assert.Equal("Alpha Game", GameScanHelper.ExtractVdfValue(vdf, "NAME"));
        Assert.Equal("Alpha Folder", GameScanHelper.ExtractVdfValue(vdf, "InstallDir"));
    }

    // Verifies that missing VDF keys return null instead of a false match.
    [Fact]
    public void ExtractVdfValue_WhenKeyIsMissing_ReturnsNull()
    {
        const string vdf = "\"AppState\"\n{\n  \"name\" \"Alpha Game\"\n}";

        var result = GameScanHelper.ExtractVdfValue(vdf, "appid");

        Assert.Null(result);
    }

    // Verifies that JSON string extraction unescapes and normalizes installer paths.
    [Fact]
    public void ExtractJsonString_WhenPathContainsEscapedBackslashes_ReturnsNormalizedPath()
    {
        const string json = "{ \"InstallLocation\": \"C:\\\\Games\\\\Alpha\\\\\" }";

        var result = GameScanHelper.ExtractJsonString(json, "InstallLocation");

        Assert.Equal($"C:{Path.DirectorySeparatorChar}Games{Path.DirectorySeparatorChar}Alpha", result);
    }

    // Verifies that missing JSON keys return null.
    [Fact]
    public void ExtractJsonString_WhenKeyIsMissing_ReturnsNull()
    {
        const string json = "{ \"DisplayName\": \"Alpha Game\" }";

        var result = GameScanHelper.ExtractJsonString(json, "InstallLocation");

        Assert.Null(result);
    }

    // Verifies that launcher tool entries and known suffixes are filtered out as non-games.
    [Theory]
    [InlineData("Steamworks Common Redistributables")]
    [InlineData("Cool _CommonRedist package")]
    [InlineData("UE_5.3")]
    [InlineData("Alpha Dedicated Server")]
    [InlineData("Alpha Demo")]
    [InlineData("Alpha Playtest")]
    public void NonGame_WhenNameMatchesKnownToolOrSuffix_ReturnsTrue(string name)
    {
        var result = GameScanHelper.NonGame(name);

        Assert.True(result);
    }

    // Verifies that regular game names are not rejected by weak substring matches.
    [Theory]
    [InlineData("Alpha Game")]
    [InlineData("Demon Souls")]
    [InlineData("BetaMax Adventure")]
    public void NonGame_WhenNameIsRegularGame_ReturnsFalse(string name)
    {
        var result = GameScanHelper.NonGame(name);

        Assert.False(result);
    }

    // Verifies that preferred executable folders stay in the expected search order.
    [Fact]
    public void GetPreferredExeSubFolders_ReturnsKnownSearchOrder()
    {
        var result = GameScanHelper.GetPreferredExeSubFolders();

        Assert.Equal(
            [
                Path.Combine("bin", "x64"),
                Path.Combine("bin", "x86"),
                Path.Combine("bin", "win64")
            ],
            result);
    }
}
