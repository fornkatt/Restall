// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;
using Restall.Infrastructure.Scanners.Heroic;
using Restall.Infrastructure.Tests.HeroicTests.Common;

namespace Restall.Infrastructure.Tests.HeroicTests;

public class EpicHeroicInstalledTests
{

    [Fact]
    public void InstalledParser_ValidEntry_ReturnsAppName()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock(
            InstalledEntry("98614687b212444c9ff0d42095f56cb3",
                @"C:\\Games\\Heroic\\Redacted"));

        // Act
        var result = HeroicInstalledParser.InstalledParser(json, Game.Platform.Epic);

        // Assert
        var actual = Assert.Single(result);
        Assert.Equal("98614687b212444c9ff0d42095f56cb3", actual.AppName);
    }

    [Fact]
    public void InstalledParser_EntryIsDLC_ReturnsTrue()
    {
         // Arrange
         var json = SharedHeroic.ReadHeroicBlock(InstalledEntry(
             "4fa3d8d9b2cb4714a19a38d1a598be8f",
             @"C:\\Games\\Heroic\\FalloutNewVegas",
             isDlc: true));

         // Act
         var result = HeroicInstalledParser.InstalledParser(json, Game.Platform.Epic);

        //Assert
         var actual = Assert.Single(result);
         Assert.True(actual.IsDlc);
    }

    [Fact]
    public void InstalledParser_MissingAppName_ReturnsEmptyAppName()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock("""
                                            "temp_app_name":
                                            { "install_path": "C:\\Games\\Heroic\\BackpackHerog4I7F",
                                            "is_dlc": false }
                                            """);
        // Act
        var result = HeroicInstalledParser.InstalledParser(json, Game.Platform.Epic);

        // Assert
        var actual = Assert.Single(result);
        Assert.Empty(actual.AppName);
    }

    [Fact]
    public void InstalledParser_MissingInstallPath_ReturnsEmptyInstallPath()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlock("""
                                            "temp_app_name":
                                            { "app_name": "c2568782280f414aa8476c9fba8bfd60",
                                            "is_dlc": false }
                                            """);
        // Act
        var result = HeroicInstalledParser.InstalledParser(json, Game.Platform.Epic);

        //Assert
        var actual = Assert.Single(result);
        Assert.Empty(actual.InstallPath);
    }

    private static string InstalledEntry(string appName, string installPath, bool isDlc = false)
        => $$"""
             "{{appName}}": { "app_name": "{{appName}}",
             "install_path": "{{installPath}}",
             "is_dlc": {{(isDlc ? "true" : "false")}} }
             """;

}
