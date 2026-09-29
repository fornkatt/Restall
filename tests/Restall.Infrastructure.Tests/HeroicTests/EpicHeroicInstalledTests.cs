// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Scanners.Heroic;
using Xunit.Sdk;

namespace Restall.Infrastructure.Tests.HeroicTests;

/// <summary>
/// Progression for my Heroic tests:
/// Regex:
/// Single Epic Fact tests
/// Inline install path tests - Valid/Unusable
/// Add additional Epic Fact tests
/// Begin GOG testing in "GOGHeroicInstalledTests"
/// Same Process
/// Look for extra things
/// Switch to json
/// </summary>
public class EpicHeroicInstalledTests
{

    [Fact]
    public void EpicHeroicParser_ValidEntry_ReturnAppName()
    {
        // Arrange
        var json = "{" + Entry("98614687b212444c9ff0d42095f56cb3", @"C:\\Games\\Heroic\\Redacted", isDlc: false) + "}";
        var expected = "98614687b212444c9ff0d42095f56cb3";
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.AppName);
    }

    [Theory]
    [InlineData(@"C:\\Games\\Heroic\\Redacted", @"C:\Games\Heroic\Redacted")]
    [InlineData(@"C:\\Games\\Heroic\\Redacted\\", @"C:\Games\Heroic\Redacted")]
    [InlineData(@"C:/Games/Heroic/Redacted", @"C:\Games\Heroic\Redacted")]
    public void EpicHeroicParser_WindowsInstallPath_ReturnNormalizedPath(string installedPath, string expected)
    {
        //Arrange
        var json = "{" + Entry("temp_appName", installedPath) + "}";
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.InstallPath);
    }



    //Checking the properties for Epic installed.json
    private static string Entry(string appName, string installPath, bool isDlc = false)
        => $$"""
             "{{appName}}": { "app_name": "{{appName}}",
             "install_path": "{{installPath}}",
             "is_dlc": {{(isDlc ? "true" : "false")}} }
             """;
}
