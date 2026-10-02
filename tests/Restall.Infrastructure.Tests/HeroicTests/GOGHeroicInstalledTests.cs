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
        var json = SharedHeroic.ReadHeroicBlockArray("\"installed\"", """
                                                                      { "appName": "1207659037",
                                                                      "install_path": "C:\\Games\\Heroic\\Alan Wake" }
                                                                      """);
        var expected = "1207659037";

        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);

        //Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.AppName);
    }

    [Fact]
    public void GOGHeroicParser_EntryIsDLC_ReturnsFalse()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlockArray("\"installed\"", """
                                                                      {
                                                                      "install_path": "C:\\Games\\Heroic\\Shadow of the Tomb Raider",
                                                                       "appName": "1356518037", "is_dlc": false }
                                                                      """);

        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);

        //Assert
        var actual = Assert.Single(result);
        Assert.False(actual.IsDlc);
    }

    [Fact]
    public void GOGHeroicParser_MissingAppName_ReturnsEmptyAppName()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlockArray("\"installed\"", """
                                                                      { "install_path": "C:\\Games\\Heroic\\Shadow of the Tomb Raider" }
                                                                      """);

        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);

        //Assert
        var actual = Assert.Single(result);
        Assert.Empty(actual.AppName);
    }

    [Fact]
    public void GOGHeroicParser_MissingInstallPath_ReturnsEmptyInstallPath()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlockArray("\"installed\"", """
                                                                      { "appName": "1356518037", "is_dlc": false }
                                                                      """);
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);

        //Assert
        var actual = Assert.Single(result);
        Assert.Empty(actual.InstallPath);
    }

}
