// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Scanners.Heroic;



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
        var json = ReadInstalledFile(Entry("98614687b212444c9ff0d42095f56cb3", @"C:\\Games\\Heroic\\Redacted"));
        var expected = "98614687b212444c9ff0d42095f56cb3";
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.AppName);
    }

    [Fact]
    public void EpicHeroicParser_WindowsInstallPath_ReturnNormalizedPath()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows pathing");

        // Arrange
        var json = ReadInstalledFile(Entry("temp_appName", @"C:\\Games\\Heroic\\Redacted\\"));
        var expected = @"C:\Games\Heroic\Redacted";
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.InstallPath);
    }

    [Fact]
    public void EpicHeroicParser_LinuxInstallPath_ReturnNormalizedPath()
    {
        Assert.SkipUnless(OperatingSystem.IsLinux(), "Linux pathing");

         // Arrange
         var json = ReadInstalledFile(Entry("temp_appName", "/home/user/Games/Heroic/AlanWake2"));
         var expected = "/home/user/Games/Heroic/AlanWake2";
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

    private static string ReadInstalledFile(params string[] entries) =>
        "{" + string.Join(",", entries) + "}";

}
