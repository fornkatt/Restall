// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXModDatabase(
    ImmutableArray<RenoDXGameMod> GameMods,
    ImmutableArray<RenoDXUnrealGenericMod> UnrealGenericMods,
    ImmutableArray<RenoDXUnityGenericMod> UnityGenericMods)
{
    public static readonly RenoDXModDatabase Empty = new([], [], []);

    public bool IsGameModListLoaded => !GameMods.IsEmpty;
}
