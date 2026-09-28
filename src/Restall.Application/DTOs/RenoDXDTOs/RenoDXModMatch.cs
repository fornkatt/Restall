// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

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

    public enum MatchKind { None, Exact, Fuzzy, Tie }
}
