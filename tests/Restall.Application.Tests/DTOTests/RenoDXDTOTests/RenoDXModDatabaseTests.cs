// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

public class RenoDXModDatabaseTests
{
    [Fact]
    public void IsGameModListLoaded_GameModsPresent_ReturnsTrue()
    {
        var database = RenoDXModDatabase.Empty with
        {
            GameMods =
            [
                new RenoDXGameMod("Test Game", RenoDXModStatus.Done, "Tester", null,
                    null, null, null, null, null)
            ]
        };

        Assert.True(database.IsGameModListLoaded);
    }

    [Fact]
    public void IsGameModListLoaded_OnlyGenericModsPresent_ReturnsFalse()
    {
        var database = RenoDXModDatabase.Empty with
        {
            UnrealGenericMods =
            [
                new RenoDXUnrealGenericMod("Test Game", RenoDXModStatus.Done,
                    RenoDXUnrealGenericMod.UnrealModMethod.Ini, null, null)
            ]
        };

        Assert.False(database.IsGameModListLoaded);
    }

    [Fact]
    public void ModDatabaseEmpty_BeforeFirstFetch_ReturnsEmptyListsAndGameModListNotLoaded()
    {
        var database = RenoDXModDatabase.Empty;

        Assert.Empty(database.GameMods);
        Assert.Empty(database.UnrealGenericMods);
        Assert.Empty(database.UnityGenericMods);
        Assert.False(database.IsGameModListLoaded);
    }
}
