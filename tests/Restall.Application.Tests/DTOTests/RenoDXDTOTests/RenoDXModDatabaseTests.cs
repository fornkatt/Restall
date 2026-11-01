// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

public class RenoDXModDatabaseTests
{
    [Fact]
    public void IsGameModListLoaded_GameModsPresentInDatabase_ReturnsTrue()
    {
        var database = new RenoDXModDatabase(
        [
            new RenoDXGameMod("Test Game", RenoDXModStatus.Done, "Tester", null,
                null, null, null, null, null)
        ], [], []);

        Assert.True(database.IsGameModListLoaded);
    }

    [Fact]
    public void IsGameModListLoaded_OnlyGenericModsPresent_ReturnsFalse()
    {
        var database = new RenoDXModDatabase([],
        [
            new RenoDXUnrealGenericMod("Test Game", RenoDXModStatus.Done,
                RenoDXUnrealGenericMod.UnrealModMethod.Ini, null, null)
        ], []);

        Assert.False(database.IsGameModListLoaded);
    }

    [Fact]
    public void ModNames_ModsInEveryList_ReturnsOnePreparedNamePerModInModOrder()
    {
        var database = new RenoDXModDatabase(
            [
                new RenoDXGameMod("First Game", RenoDXModStatus.Done,
                    null, null, null, null, null, null, null),
                new RenoDXGameMod("Second Game", RenoDXModStatus.Done,
                    null, null, null, null, null, null, null)
            ],
            [
                new RenoDXUnrealGenericMod("Unreal Game", RenoDXModStatus.Done,
                    RenoDXUnrealGenericMod.UnrealModMethod.Ini, null, null)
            ],
            [
                new RenoDXUnityGenericMod("Unity Game", RenoDXModStatus.Done, null, null)
            ]);

        Assert.Equal(["first game", "second game"], database.GameModNames.Select(n => n.NormalizedName));
        Assert.Equal(["unreal game"], database.UnrealGenericModNames.Select(n => n.NormalizedName));
        Assert.Equal(["unity game"], database.UnityGenericModNames.Select(n => n.NormalizedName));
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
