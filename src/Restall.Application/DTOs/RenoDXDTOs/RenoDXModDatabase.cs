// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed class RenoDXModDatabase
{
    public static readonly RenoDXModDatabase Empty = new([], [], []);

    public RenoDXModDatabase(
        ImmutableArray<RenoDXGameMod> gameMods,
        ImmutableArray<RenoDXUnrealGenericMod> unrealGenericMods,
        ImmutableArray<RenoDXUnityGenericMod> unityGenericMods)
    {
        GameMods = gameMods;
        UnrealGenericMods = unrealGenericMods;
        UnityGenericMods = unityGenericMods;
        GameModNames = [.. gameMods.Select(m => PreparedName.Create(m.Name))];
        UnrealGenericModNames = [.. unrealGenericMods.Select(m => PreparedName.Create(m.Name))];
        UnityGenericModNames = [.. unityGenericMods.Select(m => PreparedName.Create(m.Name))];
    }

    public ImmutableArray<RenoDXGameMod> GameMods { get; }
    public ImmutableArray<RenoDXUnrealGenericMod> UnrealGenericMods { get; }
    public ImmutableArray<RenoDXUnityGenericMod> UnityGenericMods { get; }
    public ImmutableArray<PreparedName> GameModNames { get; }
    public ImmutableArray<PreparedName> UnrealGenericModNames { get; }
    public ImmutableArray<PreparedName> UnityGenericModNames { get; }

    public bool IsGameModListLoaded => !GameMods.IsEmpty;
}
