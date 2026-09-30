// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Scanners.Heroic;
using Restall.Infrastructure.Tests.HeroicTests.Common;

namespace Restall.Infrastructure.Tests.HeroicTests;

public class GOGHeroicInstalledTests
{
    [Fact]
    public void GOGHeroicParser_ValidEntry_ReturnsAppName()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock(Entry("58459452021155541", @"C:\\Games\\Heroic\\Alan Wake"));
        var expected = "58459452021155541";
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.AppName);
    }

    [Fact(
        Skip = "Windows pathing",
        SkipUnless = nameof(SharedHeroic.IsWindows),
        SkipType = typeof(SharedHeroic)
    )]
    public void GOGHeroicParser_WindowsInstallPath_ReturnsNormalizedPath()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock(Entry("58459452021155541", @"C:\\Games\\Heroic\\Alan Wake"));
        var expected = @"C:\Games\Heroic\Alan Wake";
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.InstallPath);
    }

    [Fact]
    public void GOGHeroicParser_EntryIsDLC_ReturnsFalse()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock(Entry("58459452021155541", @"C:\\Games\\Heroic\\Alan Wake", isDlc: false));
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.False(actual.IsDlc);
    }

    [Fact]
    public void GOGHeroicParser_MissingAppName_ReturnsNullAppName()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock(Entry("", @"C:\\Games\\Heroic\\Shadow of the Tomb Raider"));
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Null(actual.AppName);
    }

    [Fact]
    public void GOGHeroicParser_MissingInstallPath_ReturnsNullInstallPath()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock(Entry("1356518037", ""));
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Null(actual.InstallPath);
    }

    //TODO: SHARED HELPER FOR EPIC AND GOG
    private static string Entry(string appName,string installPath, bool isDlc = false)
        => $$"""
             "installed": { "install_path": "{{installPath}}",
             "is_dlc": {{(isDlc ? "true" : "false")}}, "appName": "{{appName}}" }
             """;

}
