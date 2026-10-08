// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Domain.Common.Enums;
using System.Collections.Immutable;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXModMatch(
    RenoDXGameMod? GameMod,
    RenoDXUnrealGenericMod? UnrealGenericMod,
    RenoDXUnityGenericMod? UnityGenericMod,
    RenoDXModMatch.MatchKind Kind,
    ImmutableArray<string> TiedNames)
{
    public static readonly RenoDXModMatch None = new(null, null, null,
        MatchKind.None, []);

    public string? ModName => GameMod?.Name ?? UnrealGenericMod?.Name ?? UnityGenericMod?.Name;

    public RenoDXModStatus? ModStatus => GameMod?.Status ?? UnrealGenericMod?.Status ?? UnityGenericMod?.Status;

    public string? GetAddonFilename(Architecture architecture) => this switch
    {
        { GameMod: { } gameMod } => gameMod.GetAddonFilename(architecture),
        { UnrealGenericMod: not null } => RenoDXGenericAddons.GetUnrealExtendedAddonFilename(architecture),
        { UnityGenericMod: not null } => RenoDXGenericAddons.GetUnityGenericAddonFilename(architecture),
        _ => null
    };

    public enum MatchKind { None, Exact, Fuzzy, Tie }
}
