// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using System.Collections.Immutable;

namespace Restall.Application.Helpers;

public static class RenoDXModMatcher
{
    public static RenoDXModMatch Match(string? gameName, RenoDXModDatabase database)
    {
        if (string.IsNullOrWhiteSpace(gameName))
            return RenoDXModMatch.None;

        var preparedGameName = PreparedName.Create(GameNameHelper.StripCollectionPartSuffix(gameName));

        if (FindExactMatch(preparedGameName, database.GameModNames) is { } gameModIndex)
            return new RenoDXModMatch(database.GameMods[gameModIndex], null, null,
                RenoDXModMatch.MatchKind.Exact, []);

        if (FindExactMatch(preparedGameName, database.UnrealGenericModNames) is { } unrealModIndex)
            return new RenoDXModMatch(null, database.UnrealGenericMods[unrealModIndex],
                null, RenoDXModMatch.MatchKind.Exact, []);

        if (FindExactMatch(preparedGameName, database.UnityGenericModNames) is { } unityModIndex)
            return new RenoDXModMatch(null, null, database.UnityGenericMods[unityModIndex],
                RenoDXModMatch.MatchKind.Exact, []);

        var gameModCandidates = FindClosestFuzzyMatches(preparedGameName, database.GameModNames);

        switch (gameModCandidates.Length)
        {
            case 1:
                return new RenoDXModMatch(database.GameMods[gameModCandidates[0]], null, null,
                    RenoDXModMatch.MatchKind.Fuzzy, []);
            case > 1:
                return CreateTie([.. gameModCandidates.Select(index => database.GameMods[index].Name)]);
        }

        var unrealModCandidates = FindClosestFuzzyMatches(preparedGameName, database.UnrealGenericModNames);

        switch (unrealModCandidates.Length)
        {
            case 1:
                return new RenoDXModMatch(null, database.UnrealGenericMods[unrealModCandidates[0]], null,
                    RenoDXModMatch.MatchKind.Fuzzy, []);
            case > 1:
                return CreateTie([.. unrealModCandidates.Select(index => database.UnrealGenericMods[index].Name)]);
        }

        var unityModCandidates = FindClosestFuzzyMatches(preparedGameName, database.UnityGenericModNames);

        return unityModCandidates.Length switch
        {
            1 => new RenoDXModMatch(null, null, database.UnityGenericMods[unityModCandidates[0]],
                RenoDXModMatch.MatchKind.Fuzzy, []),
            > 1 => CreateTie([.. unityModCandidates.Select(index => database.UnityGenericMods[index].Name)]),
            _ => RenoDXModMatch.None
        };
    }

    private static int? FindExactMatch(PreparedName gameName, ImmutableArray<PreparedName> modNames)
    {
        for (var i = 0; i < modNames.Length; i++)
        {
            if (modNames[i].NormalizedName == gameName.NormalizedName)
                return i;
        }

        return null;
    }

    private static ImmutableArray<int> FindClosestFuzzyMatches(PreparedName gameName,
        ImmutableArray<PreparedName> modNames)
    {
        var closestMatches = ImmutableArray.CreateBuilder<int>();
        var highestOverlap = 0;

        for (var i = 0; i < modNames.Length; i++)
        {
            if (!GameNameHelper.IsLikelySameGame(gameName, modNames[i]))
                continue;

            var overlap = GameNameHelper.CountSharedWords(gameName, modNames[i]);

            if (overlap > highestOverlap)
            {
                highestOverlap = overlap;
                closestMatches.Clear();
            }

            if (overlap == highestOverlap)
                closestMatches.Add(i);
        }

        return closestMatches.ToImmutable();
    }

    private static RenoDXModMatch CreateTie(ImmutableArray<string> tiedNames) =>
        new(null, null, null, RenoDXModMatch.MatchKind.Tie, tiedNames);
}
