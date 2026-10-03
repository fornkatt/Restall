// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Helpers;


namespace Restall.Infrastructure.Tests.HelpersTests;

public class JsonStringTests
{
    [Fact]
    public void ExtractJsonString_InstallLocationTrailingSeparator_ReturnsUnchanged()
    {
         // Arrange
        var json = """{ "InstallLocation": "C:\\Epic Games\\LEGOStarWarsTSS\\" }""";
        var expected = @"C:\Epic Games\LEGOStarWarsTSS\";

        // Act
        var actual = GameScanHelper.ExtractJsonString(json, "InstallLocation");

        //Assert
        Assert.Equal(expected, actual);
    }


}
