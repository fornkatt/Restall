using Restall.Infrastructure.Helpers;

namespace Restall.Tests.Infrastructure;

public sealed class RegexHelperTests
{
    // Verifies that RenoDX file-version strings expose the expected date parts.
    [Fact]
    public void RenoDXVersionRegex_WhenVersionHasExpectedShape_CapturesYearAndDay()
    {
        var match = RegexHelper.RenoDXVersionRegex.Match("1.2024.0203.0");

        Assert.True(match.Success);
        Assert.Equal("2024", match.Groups[1].Value);
        Assert.Equal("0203", match.Groups[2].Value);
    }

    // Verifies that invalid RenoDX file-version strings do not match.
    [Fact]
    public void RenoDXVersionRegex_WhenVersionHasUnexpectedShape_DoesNotMatch()
    {
        var match = RegexHelper.RenoDXVersionRegex.Match("2024.02.03");

        Assert.False(match.Success);
    }

    // Verifies that the ReShade site regex extracts the semantic version from page text.
    [Fact]
    public void ExtractReShadeVersionFromSite_WhenTextContainsVersion_CapturesVersion()
    {
        var match = RegexHelper.ExtractReShadeVersionFromSite.Match("Download ReShade 6.5.0 with full add-on support");

        Assert.True(match.Success);
        Assert.Equal("6.5.0", match.Groups[1].Value);
    }

    // Verifies that Steam library regex captures VDF library paths.
    [Fact]
    public void SteamLibraryRegex_WhenPathEntryExists_CapturesPath()
    {
        const string vdf = "\"libraryfolders\"\n{\n  \"1\"\n  {\n    \"path\" \"D:\\\\SteamLibrary\"\n  }\n}";

        var match = RegexHelper.SteamLibraryRegex.Match(vdf);

        Assert.True(match.Success);
        Assert.Equal("D:\\\\SteamLibrary", match.Groups[1].Value);
    }

    // Verifies that Heroic regexes extract game block details from installed.json content.
    [Fact]
    public void HeroicRegexes_WhenBlockContainsGameDetails_CaptureExpectedValues()
    {
        const string json = """
        {
          "alpha": { "appName": "alpha-app", "title": "Alpha Game", "install_path": "C:\\Games\\Alpha" },
          "ignored": { "title": "No install path" }
        }
        """;

        var blockMatch = RegexHelper.HeroicGameBlockRegex.Match(json);
        Assert.True(blockMatch.Success);

        var block = blockMatch.Value;
        Assert.Equal("alpha-app", RegexHelper.HeroicAppNameRegex.Match(block).Groups[1].Value);
        Assert.Equal("Alpha Game", RegexHelper.HeroicTitleRegex.Match(block).Groups[1].Value);
        Assert.Equal("C:\\\\Games\\\\Alpha", RegexHelper.HeroicInstallPathRegex.Match(block).Groups[1].Value);
    }
}
