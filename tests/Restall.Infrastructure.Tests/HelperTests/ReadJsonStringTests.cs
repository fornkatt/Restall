// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Helpers;
using System.Text.Json;

namespace Restall.Infrastructure.Tests.HelperTests;

public class ReadJsonStringTests
{

    [Fact]
    public void ReadJsonString_UnicodeEscapedCharacterInTitle_ReturnsDecoded()
    {
        // Arrange
        var json = """{ "DisplayName": "LEGO\u00ae Star Wars\u2122: The Skywalker Saga" }""";
        var expected = "LEGO® Star Wars™: The Skywalker Saga";

        // Act
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var actual = GameScanHelper.ReadJsonString(root, "DisplayName");

        //Assert
        Assert.Equal(expected, actual);
    }

}
