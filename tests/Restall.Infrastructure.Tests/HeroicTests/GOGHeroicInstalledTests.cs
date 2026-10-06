// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;
using Restall.Infrastructure.Scanners.Heroic;
using Restall.Infrastructure.Tests.HeroicTests.Common;

namespace Restall.Infrastructure.Tests.HeroicTests;

public class GOGHeroicInstalledTests
{
    [Fact]
    public void InstalledParser_ValidEntry_ReturnsAppName()
    {
        // Arrange
        var json = SharedHeroic.ReadInstalledBlockArray("\"installed\"", """
                                                                      { "appName": "1207659037",
                                                                      "install_path": "C:\\Games\\Heroic\\Alan Wake" }
                                                                      """);
        // Act
        var result = HeroicInstalledParser.InstalledParser(json, Game.Platform.GOG);

        //Assert
        var actual = Assert.Single(result);
        Assert.Equal("1207659037", actual.AppName);
    }

    [Fact]
    public void InstalledParser_EntryIsDLC_ReturnsFalse()
    {
        // Arrange
        var json = SharedHeroic.ReadInstalledBlockArray("\"installed\"", """
                                                                      {
                                                                      "install_path": "C:\\Games\\Heroic\\Shadow of the Tomb Raider",
                                                                       "appName": "1356518037", "is_dlc": false }
                                                                      """);
        // Act
        var result = HeroicInstalledParser.InstalledParser(json, Game.Platform.GOG);

        //Assert
        var actual = Assert.Single(result);
        Assert.False(actual.IsDlc);
    }

    [Fact]
    public void InstalledParser_MissingAppName_ReturnsEmptyAppName()
    {
        // Arrange
        var json = SharedHeroic.ReadInstalledBlockArray("\"installed\"", """
                                                                      { "install_path": "C:\\Games\\Heroic\\Shadow of the Tomb Raider" }
                                                                      """);

        // Act
        var result = HeroicInstalledParser.InstalledParser(json, Game.Platform.GOG);

        //Assert
        var actual = Assert.Single(result);
        Assert.Empty(actual.AppName);
    }

    [Fact]
    public void InstalledParser_MissingInstallPath_ReturnsEmptyInstallPath()
    {
        // Arrange
        var json = SharedHeroic.ReadInstalledBlockArray("\"installed\"", """
                                                                      { "appName": "1356518037", "is_dlc": false }
                                                                      """);
        // Act
        var result = HeroicInstalledParser.InstalledParser(json, Game.Platform.GOG);

        //Assert
        var actual = Assert.Single(result);
        Assert.Empty(actual.InstallPath);
    }

}
