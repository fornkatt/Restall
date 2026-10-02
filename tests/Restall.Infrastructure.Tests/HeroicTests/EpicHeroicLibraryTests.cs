// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Scanners.Heroic;
using Restall.Infrastructure.Tests.HeroicTests.Common;

namespace Restall.Infrastructure.Tests.HeroicTests;

public class EpicHeroicLibraryTests
{
    [Fact]
    public void EpicLibraryParser_CheckValidEntry_ReturnsAppNameAndTitle()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlockArray("\"library\"", """
                                                                  { "app_name": "98614687b212444c9ff0d42095f56cb3", "title": "[REDACTED]" }
                                                                  """);
        // Act
        var result = HeroicLibraryParser.EpicLibraryParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Equal("98614687b212444c9ff0d42095f56cb3", actual.AppName);
        Assert.Equal("[REDACTED]", actual.Title);
    }

    [Fact]
    public void EpicLibraryParser_MissingAppName_ReturnsEmpty()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlockArray("\"library\"", """
                                                                  { "app_name": "", "title": "[REDACTED]" }
                                                                  """);
        // Act
        var result = HeroicLibraryParser.EpicLibraryParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Empty(actual.AppName);
    }

    [Fact]
    public void EpicLibraryParser_MissingTitle_ReturnsEmpty()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlockArray("\"library\"", """
                                                                  { "app_name": "98614687b212444c9ff0d42095f56cb3", "title": "" }
                                                                  """);
        // Act
        var result = HeroicLibraryParser.EpicLibraryParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Empty(actual.Title);
    }

    [Fact]
    public void EpicLibraryParser_UnicodeCharacterInTitle_ReturnsDecoded()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlockArray("\"library\"", """
                                                                    { "app_name": "4fa3d8d9b2cb4714a19a38d1a598be8f", "title": "Fallout New Vegas\u00ae: Lonesome Road\u2122" }
                                                                    """);
        var expected = "Fallout New Vegas®: Lonesome Road™";
        // Act
        var result = HeroicLibraryParser.EpicLibraryParser(json);
        // Assert
        var actual = Assert.Single(result);
        Assert.Equal(expected, actual.Title);
    }



}






