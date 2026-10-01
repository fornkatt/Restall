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
        var json = """{ "installed": [ { "appName": "1207659037", "install_path": "C:\\Games\\Heroic\\Alan Wake" } ] }""";
        var expected = "1207659037";
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
        var json = """{ "installed": [ { "appName": "1207659037", "install_path": "C:\\Games\\Heroic\\Alan Wake\\" } ] }""";
        var expected = @"C:\Games\Heroic\Alan Wake";
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.InstallPath);
    }

    [Fact(
        Skip = "Linux pathing",
        SkipUnless = nameof(SharedHeroic.IsLinux),
        SkipType = typeof(SharedHeroic)
    )]
    public void GOGHeroicParser_LinuxInstallPath_ReturnsNormalizedPath()
    {
        // Arrange
        var json = """{ "installed": [ { "appName": "1495134320", "install_path": "/home/user/Games/Heroic/The Witcher 3 Wild Hunt GOTY" } ] }""";
        var expected = @"/home/user/Games/Heroic/The Witcher 3 Wild Hunt GOTY";
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
        var json = """{ "installed": [ { "install_path": "C:\\Games\\Heroic\\Shadow of the Tomb Raider", "appName": "1356518037", "is_dlc": false } ] }""";
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
        var json = """{ "installed": [ { "install_path": "C:\\Games\\Heroic\\Shadow of the Tomb Raider" } ] }""";
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
        var json = """{ "installed": [ { "appName": "1356518037", "is_dlc": false } ] }""";
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Null(actual.InstallPath);
    }


}
