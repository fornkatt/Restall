// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;
using Restall.Infrastructure.Scanners.Heroic;
using Restall.Infrastructure.Tests.HeroicTests.Common;

namespace Restall.Infrastructure.Tests.HeroicTests;

public class HeroicLibraryTests
{
    public static TheoryData<Game.Platform, string, string> ValidEntries => new()
    {
        { Game.Platform.Epic, "98614687b212444c9ff0d42095f56cb3", "[REDACTED]" },
        { Game.Platform.GOG, "1207659037", "Alan Wake" }
    };

    public static TheoryData<Game.Platform, string> MissingAppNames(string entry) => new()
    {
        { Game.Platform.Epic, entry },
        { Game.Platform.GOG, entry }
    };

    public static TheoryData<Game.Platform, string> MissingTitles => new()
    {
        { Game.Platform.Epic, """{ "app_name": "98614687b212444c9ff0d42095f56cb3" }""" },
        { Game.Platform.GOG, """{ "app_name": "1207659037" }"""  }
    };

    [Theory]
    [MemberData(nameof(ValidEntries))]
    public void LibraryParser_CheckValidEntries_ReturnsAppNameAndTitle(Game.Platform platform, string appName,
        string title)
    {
        // Arrange
        var json = LibraryArray(platform, LibraryEntry(appName, title));
        // Act
        var result = HeroicLibraryParser.LibraryParser(json, platform);
        //Assert
        var actual = Assert.Single(result);
        Assert.Equal(appName, actual.AppName);
        Assert.Equal(title, actual.Title);
    }

    [Theory]
    [MemberData(nameof(MissingAppNames), """{ "title": "Alan Wake" }""")]
    public void LibraryParser_MissingAppName_ReturnsEmpty(Game.Platform platform, string appName)
    {
        // Arrange
        var json = LibraryArray(platform, appName);
        // Act
        var result = HeroicLibraryParser.LibraryParser(json, platform);
        //Assert
        var actual = Assert.Single(result);
        Assert.Empty(actual.AppName);

    }

    [Theory]
    [MemberData(nameof(MissingTitles))]
    public void LibraryParser_MissingTitle_ReturnsEmpty(Game.Platform platform, string title)
    {
        // Arrange
        var json = LibraryArray(platform, title);
        // Act
        var result = HeroicLibraryParser.LibraryParser(json, platform);
        //Assert
        var actual = Assert.Single(result);
        Assert.Empty(actual.Title);

    }

    private static string LibraryEntry(string appName, string title) =>
        $$"""{ "app_name": "{{appName}}", "title": "{{title}}" }""";

    private static string LibraryArray(Game.Platform platform, params string[] entries)
        => SharedHeroic.ReadInstalledBlockArray(platform == Game.Platform.GOG ? "\"games\"" : "\"library\"", entries);


}






