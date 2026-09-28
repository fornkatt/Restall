// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

public class RenoDXModMatchTests
{
    [Fact]
    public void ModMatchNone_NoDatabaseEntry_ReturnsNothing()
    {
        var noMatch = RenoDXModMatch.None;

        Assert.Null(noMatch.GameMod);
        Assert.Null(noMatch.UnrealGenericMod);
        Assert.Null(noMatch.UnityGenericMod);
        Assert.Equal(RenoDXModMatch.MatchKind.None, noMatch.Kind);
        Assert.Empty(noMatch.TiedNames);
    }
}
