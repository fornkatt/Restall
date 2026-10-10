// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging.Abstractions;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Services;

namespace Restall.Application.Tests.ServiceTests;

public class RenoDXDatabaseBuilderTests
{
    private static readonly RenoDXGameMod s_validGameMod =
        new("Test Game", RenoDXModStatus.Done, null, null, null, null,
            null, null, null);

    private static readonly RenoDXUnrealGenericMod s_validUnrealGenericMod =
        new("Test Game", RenoDXModStatus.Done, RenoDXUnrealGenericMod.UnrealModMethod.Native, null,
            null);

    private static readonly RenoDXUnityGenericMod s_validUnityGenericMod =
        new("Test Game", RenoDXModStatus.Done, null, null);

    [Fact]
    public void Build_UsableEntryInEveryList_ReturnsSuccess()
    {
        var sut = CreateBuilder();

        var actual = sut.Build([s_validGameMod], [s_validUnrealGenericMod],
            [s_validUnityGenericMod]);

        Assert.True(actual.IsSuccess);
        Assert.False(actual.IsPartial);
        Assert.NotNull(actual.Value);
        Assert.Equal(s_validGameMod, Assert.Single(actual.Value.GameMods));
        Assert.Equal(s_validUnrealGenericMod, Assert.Single(actual.Value.UnrealGenericMods));
        Assert.Equal(s_validUnityGenericMod, Assert.Single(actual.Value.UnityGenericMods));
    }

    [Fact]
    public void Build_UnusableEntry_ReturnsPartialWithoutUnusableEntry()
    {
        var sut = CreateBuilder();
        var unusableGameMod = s_validGameMod with { Name = " " };

        var actual = sut.Build([s_validGameMod, unusableGameMod], [s_validUnrealGenericMod],
            [s_validUnityGenericMod]);

        Assert.True(actual.IsPartial);
        Assert.Equal(WarningType.RenoDXModDatabaseIncomplete, actual.WarningType);
        Assert.NotNull(actual.Value);
        Assert.Equal(s_validGameMod, Assert.Single(actual.Value.GameMods));
    }

    [Fact]
    public void Build_GameModsListEmpty_ReturnsPartialWithIsGameModListLoadedFalse()
    {
        var sut = CreateBuilder();

        var actual = sut.Build([], [s_validUnrealGenericMod],
            [s_validUnityGenericMod]);

        Assert.True(actual.IsPartial);
        Assert.Equal(WarningType.RenoDXModDatabaseIncomplete, actual.WarningType);
        Assert.NotNull(actual.Value);
        Assert.False(actual.Value.IsGameModListLoaded);
    }

    [Fact]
    public void Build_AllListsEmpty_ReturnsError()
    {
        var sut = CreateBuilder();

        var actual = sut.Build([], [], []);

        Assert.False(actual.IsSuccess);
        Assert.Equal(ErrorType.RenoDXModDatabaseUnavailable, actual.ErrorType);
    }

    [Fact]
    public void Build_OutOfRangeStatusEntry_ReturnsPartialWithoutOutOfRangeStatusEntry()
    {
        var entryWithOutOfRangeStatus = s_validGameMod with { Status = (RenoDXModStatus)99 };
        var sut = CreateBuilder();

        var actual = sut.Build([s_validGameMod, entryWithOutOfRangeStatus],
            [s_validUnrealGenericMod], [s_validUnityGenericMod]);

        Assert.True(actual.IsSuccess);
        Assert.True(actual.IsPartial);
        Assert.Equal(WarningType.RenoDXModDatabaseIncomplete, actual.WarningType);
        Assert.NotNull(actual.Value);
        Assert.Equal(s_validGameMod, Assert.Single(actual.Value.GameMods));
    }

    private static RenoDXModDatabaseBuilder CreateBuilder() =>
        new(NullLogger<RenoDXModDatabaseBuilder>.Instance);
}
