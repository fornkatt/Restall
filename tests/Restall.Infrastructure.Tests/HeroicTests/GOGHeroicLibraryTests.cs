// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Scanners.Heroic;
using Restall.Infrastructure.Tests.HeroicTests.Common;

namespace Restall.Infrastructure.Tests.HeroicTests;

public class GOGHeroicLibraryTests
{
    [Fact]
    public void GOGLibraryParser_CheckValidEntry_ReturnsAppNameAndTitle()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlockArray("\"games\"", """
                                                                      { "app_name": "1207659037", "title": "Alan Wake" }
                                                                      """);
        // Act
        var result = HeroicLibraryParser.GOGLibraryParser(json);

        //Assert
        var actual = Assert.Single(result);
        Assert.Equal("1207659037", actual.AppName);
        Assert.Equal("Alan Wake", actual.Title);


    }

    [Fact]
    public void GOGLibraryParser_MissingAppName_ReturnsEmpty()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlockArray("\"games\"", """
                                                                      { "app_name": "", "title": "Alan Wake" }
                                                                      """);
        // Act
        var result = HeroicLibraryParser.GOGLibraryParser(json);
        //Assert
        Assert.Empty(result);

    }

    [Fact]
    public void GOGLibraryParser_MissingTitle_ReturnsEmpty()
    {
        // Arrange
        var json = SharedHeroic.ReadHeroicBlockArray("\"games\"", """
                                                                      { "app_name": "1207659037", "title": "" }
                                                                      """);
        // Act
        var result = HeroicLibraryParser.GOGLibraryParser(json);
        //Assert
        Assert.Empty(result);
    }

}
