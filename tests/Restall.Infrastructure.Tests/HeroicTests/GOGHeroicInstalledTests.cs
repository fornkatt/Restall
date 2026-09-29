// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Scanners.Heroic;

namespace Restall.Infrastructure.Tests.HeroicTests;

public class GOGHeroicInstalledTests
{
    [Fact]
    public void GOGHeroicParser_ValidEntry_ReturnsAppName()
    {
        // Arrange
        var json = ReadInstalledFile(Entry("58459452021155541", @"C:\\Games\\Heroic\\Alan Wake"));
        var expected = "58459452021155541";
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.AppName);
    }

    [Fact]
    public void GOGHeroicParser_WindowsInstallPath_ReturnsNormalizedPath()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows pathing");
        // Arrange
        var json = ReadInstalledFile(Entry("58459452021155541", @"C:\\Games\\Heroic\\Alan Wake"));
        var expected = @"C:\Games\Heroic\Alan Wake";
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.InstallPath);
    }

    [Fact]
    public void GOGHeroicParser_CheckIfEntryIsNotDLC_ReturnsFalse()
    {
        // Arrange
        var json = ReadInstalledFile(Entry("58459452021155541", @"C:\\Games\\Heroic\\Alan Wake", isDlc: false));
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.False(actual.IsDlc);
    }

    [Fact]
    public void GOGHeroicParser_CheckIfEntryIsMissing_ReturnsNull()
    {
        // Arrange
        var json = ReadInstalledFile(Entry("", @"C:\\Games\\Heroic\\Shadow of the Tomb Raider"));
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Null(actual.AppName);
    }

    [Fact]
    public void GOGHeroicParser_CheckIfInstallPathIsMissing_ReturnsNull()
    {
        // Arrange
        var json = ReadInstalledFile(Entry("1356518037", ""));
        // Act
        var result = HeroicInstalledParser.GOGHeroicParser(json);
        //Assert
        var actual = Assert.Single(result);
        Assert.Null(actual.InstallPath);
    }


    private static string Entry(string appName,string installPath, bool isDlc = false)
        => $$"""
             "installed": { "install_path": "{{installPath}}",
             "is_dlc": {{(isDlc ? "true" : "false")}}, "appName": "{{appName}}" }
             """;

    private static string ReadInstalledFile(params string[] entries) =>
        "{" + string.Join(",", entries) + "}";
}
