// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;
using System.Text.RegularExpressions;

namespace Restall.Application.Helpers;

public static partial class ArchitectureRecommender
{
    [GeneratedRegex(@"\b32[\s-]?bit\b", RegexOptions.IgnoreCase)]
    private static partial Regex ThirtyTwoBitRegex();

    public static Architecture Recommend(ReShade? installedReShade, RenoDX? installedRenoDX, RenoDXModMatch match) =>
        installedReShade?.Arch
        ?? installedRenoDX?.Arch
        ?? RecommendFromRenoDXMatch(match)
        ?? Architecture.X64;

    public static bool Mentions32Bit(string? text) =>
        text is not null && ThirtyTwoBitRegex().IsMatch(text);

    private static Architecture? RecommendFromRenoDXMatch(RenoDXModMatch match) => match switch
    {
        { GameMod: { } gameMod } => RecommendFromRenoDXGameMod(gameMod),
        { UnrealGenericMod: not null } => Architecture.X64,
        { UnityGenericMod: { } unityGenericMod } => RecommendFromRenoDXUnityGenericMod(unityGenericMod),
        _ => null
    };

    private static Architecture RecommendFromRenoDXGameMod(RenoDXGameMod gameMod) =>
        gameMod.AddonFilename is null && gameMod.AddonFilename32 is not null
            ? Architecture.X32
            : Architecture.X64;

    private static Architecture RecommendFromRenoDXUnityGenericMod(RenoDXUnityGenericMod unityGenericMod) =>
        Mentions32Bit(unityGenericMod.Comments)
            ? Architecture.X32
            : Architecture.X64;
}
