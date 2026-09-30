// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Scanners.Heroic;
using Restall.Infrastructure.Tests.HeroicTests.Common;



namespace Restall.Infrastructure.Tests.HeroicTests;


/// <summary>
/// First iteration of tests done
/// Write JSON-only tests, confirm they are red on Regex
/// Switch Parser to JSON on Epic
/// All tests are green
/// Do the same process with GOG
/// Move to library process
/// </summary>

public class EpicHeroicInstalledTests
{

    [Fact]
    public void EpicHeroicParser_ValidEntry_ReturnsAppName()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock(Entry("98614687b212444c9ff0d42095f56cb3", @"C:\\Games\\Heroic\\Redacted"));
        var expected = "98614687b212444c9ff0d42095f56cb3";
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.AppName);
    }

    [Fact(
        Skip = "Windows pathing",
        SkipUnless = nameof(SharedHeroic.IsWindows),
        SkipType = typeof(SharedHeroic)
        )]
    public void EpicHeroicParser_WindowsInstallPath_ReturnsNormalizedPath()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock(Entry("temp_appName", @"C:\\Games\\Heroic\\Redacted\\"));
        var expected = @"C:\Games\Heroic\Redacted";
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.InstallPath);
    }

    [Fact(
        Skip = "Linux pathing",
        SkipUnless = nameof(SharedHeroic.IsLinux),
        SkipType = typeof(SharedHeroic)
    )]
    public void EpicHeroicParser_LinuxInstallPath_ReturnsNormalizedPath()
    {
         // Arrange
         var json = SharedHeroic.ReadHeroicBlock(Entry("temp_appName", "/home/user/Games/Heroic/AlanWake2"));
         var expected = "/home/user/Games/Heroic/AlanWake2";
         // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
         //Assert
         var actual = Assert.Single(result);
         Assert.Equal(expected, actual.InstallPath);

    }

    [Fact]
    public void EpicHeroicParser_EntryIsDLC_ReturnsTrue()
    {
         // Arrange
         var json = SharedHeroic.ReadHeroicBlock(Entry("4fa3d8d9b2cb4714a19a38d1a598be8f", @"C:\\Games\\Heroic\\FalloutNewVegas", isDlc: true));
         // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
         //Assert
         var actual = Assert.Single(result);
         Assert.True(actual.IsDlc);
    }

    [Fact]
    public void EpicHeroicParser_MissingAppName_ReturnsNullAppName()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock("""
                                            "c2568782280f414aa8476c9fba8bfd60":
                                            { "install_path": "C:\\Games\\Heroic\\BackpackHerog4I7F",
                                            "is_dlc": false }
                                            """);
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Null(actual.AppName);
    }

    [Fact]
    public void EpicHeroicParser_MissingInstallPath_ReturnsNullInstallPath()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock("""
                                            "c2568782280f414aa8476c9fba8bfd60":
                                            { "app_name": "c2568782280f414aa8476c9fba8bfd60",
                                            "is_dlc": false }
                                            """);

        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Null(actual.InstallPath);
    }

    [Fact]
    public void EpicHeroicParser_UnicodeCharactersInTitle_ReturnsRemovedCharacters()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock("""
                                            "c2568782280f414aa8476c9fba8bfd60":
                                            {
                                            "install_path": "C:\\Games\\Heroic\\FallOutNewVegas",
                                            "title": "Fallout New Vegas\u00ae: Lonesome Road\u2122" }
                                            """);
        var expected = "Fallout New Vegas®: Lonesome Road™";

        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);

        //Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.Title);
    }

    //Checking the properties for Epic installed.json
    private static string Entry(string appName, string installPath, bool isDlc = false)
        => $$"""
             "{{appName}}": { "app_name": "{{appName}}",
             "install_path": "{{installPath}}",
             "is_dlc": {{(isDlc ? "true" : "false")}} }
             """;

}
