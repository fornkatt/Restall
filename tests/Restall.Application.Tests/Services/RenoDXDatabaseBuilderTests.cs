// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging.Abstractions;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Services;

namespace Restall.Application.Tests.Services;

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

        var result = sut.Build([s_validGameMod], [s_validUnrealGenericMod],
            [s_validUnityGenericMod]);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsPartial);
        Assert.NotNull(result.Value);
        Assert.Equal(s_validGameMod, Assert.Single(result.Value.GameMods));
        Assert.Equal(s_validUnrealGenericMod, Assert.Single(result.Value.UnrealGenericMods));
        Assert.Equal(s_validUnityGenericMod, Assert.Single(result.Value.UnityGenericMods));
    }

    [Fact]
    public void Build_UnusableEntry_ReturnsPartialWithoutUnusableEntry()
    {
        var sut = CreateBuilder();
        var unusableGameMod = s_validGameMod with { Name = " " };

        var result = sut.Build([s_validGameMod, unusableGameMod], [s_validUnrealGenericMod],
            [s_validUnityGenericMod]);

        Assert.True(result.IsPartial);
        Assert.Equal(WarningType.RenoDXModDatabaseIncomplete, result.WarningType);
        Assert.NotNull(result.Value);
        Assert.Equal(s_validGameMod, Assert.Single(result.Value.GameMods));
    }

    [Fact]
    public void Build_GameModsListEmpty_ReturnsPartialWithIsGameModListLoadedFalse()
    {
        var sut = CreateBuilder();

        var result = sut.Build([], [s_validUnrealGenericMod],
            [s_validUnityGenericMod]);

        Assert.True(result.IsPartial);
        Assert.Equal(WarningType.RenoDXModDatabaseIncomplete, result.WarningType);
        Assert.NotNull(result.Value);
        Assert.False(result.Value.IsGameModListLoaded);
    }

    [Fact]
    public void Build_AllListsEmpty_ReturnsError()
    {
        var sut = CreateBuilder();

        var result = sut.Build([], [], []);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.RenoDXModDatabaseUnavailable, result.ErrorType);
    }

    private static RenoDXModDatabaseBuilder CreateBuilder() =>
        new(NullLogger<RenoDXModDatabaseBuilder>.Instance);
}
