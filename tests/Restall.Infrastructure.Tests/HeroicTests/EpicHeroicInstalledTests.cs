// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Scanners.Heroic;



namespace Restall.Infrastructure.Tests.HeroicTests;

public static class SharedHelpers
{
    public static bool IsLinux => OperatingSystem.IsLinux();
    public static bool IsWindows => OperatingSystem.IsWindows();

    public static string ReadInstalledFile(params string[] entries) =>
        "{" + string.Join(",", entries) + "}";
}


public class EpicHeroicInstalledTests
{

    [Fact]
    public void EpicHeroicParser_ValidEntry_ReturnsAppName()
    {
        // Arrange
        var json = SharedHelpers.ReadInstalledFile(Entry("98614687b212444c9ff0d42095f56cb3", @"C:\\Games\\Heroic\\Redacted"));
        var expected = "98614687b212444c9ff0d42095f56cb3";
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.AppName);
    }

    [Fact(
        Skip = "Windows pathing",
        SkipUnless = nameof(SharedHelpers.IsWindows),
        SkipType = typeof(SharedHelpers)
        )]
    public void EpicHeroicParser_WindowsInstallPath_ReturnsNormalizedPath()
    {
        // Arrange
        var json = SharedHelpers.ReadInstalledFile(Entry("temp_appName", @"C:\\Games\\Heroic\\Redacted\\"));
        var expected = @"C:\Games\Heroic\Redacted";
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.InstallPath);
    }

    [Fact(
        Skip = "Linux pathing",
        SkipUnless = nameof(SharedHelpers.IsLinux),
        SkipType = typeof(SharedHelpers)
    )]
    public void EpicHeroicParser_LinuxInstallPath_ReturnsNormalizedPath()
    {
         // Arrange
         var json = SharedHelpers.ReadInstalledFile(Entry("temp_appName", "/home/user/Games/Heroic/AlanWake2"));
         var expected = "/home/user/Games/Heroic/AlanWake2";
         // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
         //Assert
         var actual = Assert.Single(result);
         Assert.Equal(expected, actual.InstallPath);

    }

    [Fact]
    public void EpicHeroicParser_CheckIfEntryIsDLC_ReturnsTrue()
    {
         // Arrange
         var json = SharedHelpers.ReadInstalledFile(Entry("4fa3d8d9b2cb4714a19a38d1a598be8f", @"C:\\Games\\Heroic\\FalloutNewVegas", isDlc: true));
         // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
         //Assert
         var actual = Assert.Single(result);
         Assert.True(actual.IsDlc);
    }

    [Fact]
    public void EpicHeroicParser_CheckIfEntryIsMissing_ReturnsNull()
    {
        // Arrange
        var json = SharedHelpers.ReadInstalledFile(Entry("",@"C:\\Games\\Heroic\\BackpackHerog4I7F"));
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Null(actual.AppName);
    }

    [Fact]
    public void EpicHeroicParser_CheckIfInstallPathIsMissing_ReturnsNull()
    {
        // Arrange
        var json = SharedHelpers.ReadInstalledFile(Entry("c2568782280f414aa8476c9fba8bfd60", ""));
        // Act
        var result = HeroicInstalledParser.EpicHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Null(actual.InstallPath);
    }

    //Checking the properties for Epic installed.json
    private static string Entry(string appName, string installPath, bool isDlc = false)
        => $$"""
             "{{appName}}": { "app_name": "{{appName}}",
             "install_path": "{{installPath}}",
             "is_dlc": {{(isDlc ? "true" : "false")}} }
             """;

}
