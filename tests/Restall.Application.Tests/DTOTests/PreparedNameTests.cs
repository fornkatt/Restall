// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;

namespace Restall.Application.Tests.DTOTests;

public class PreparedNameTests
{
    [Fact]
    public void Create_NameWithAccentAndInWordHyphen_ReturnsWordSplitAndNormalizedNames()
    {
        var actual = PreparedName.Create("Pokémon™ Half-Life: Arceus");

        Assert.Equal("Pokemon™ Half Life: Arceus", actual.WordSplitName);
        Assert.Equal("pokemon half life arceus", actual.NormalizedName);
    }
}
