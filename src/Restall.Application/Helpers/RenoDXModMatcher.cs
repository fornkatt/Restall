// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using System.Collections.Immutable;

namespace Restall.Application.Helpers;

public static class RenoDXModMatcher
{
    public static RenoDXModMatch Match(string? gameName, RenoDXModDatabase database)
    {
        if (string.IsNullOrWhiteSpace(gameName))
            return RenoDXModMatch.None;

        var preparedGameName = PrepareName(GameNameHelper.StripCollectionPartSuffix(gameName));

        var gameModNames = PrepareNames(database.GameMods.Select(m => m.Name));

        if (FindExactMatch(preparedGameName, gameModNames) is { } gameModIndex)
            return new RenoDXModMatch(database.GameMods[gameModIndex], null, null,
                RenoDXModMatch.MatchKind.Exact, []);

        var unrealModNames = PrepareNames(database.UnrealGenericMods
            .Select(m => m.Name));

        if (FindExactMatch(preparedGameName, unrealModNames) is { } unrealModIndex)
            return new RenoDXModMatch(null, database.UnrealGenericMods[unrealModIndex],
                null, RenoDXModMatch.MatchKind.Exact, []);

        var unityModNames = PrepareNames(database.UnityGenericMods
            .Select(m => m.Name));

        if (FindExactMatch(preparedGameName, unityModNames) is { } unityModIndex)
            return new RenoDXModMatch(null, null, database.UnityGenericMods[unityModIndex],
                RenoDXModMatch.MatchKind.Exact, []);

        var gameModCandidates = FindClosestFuzzyMatches(preparedGameName, gameModNames);

        switch (gameModCandidates.Length)
        {
            case 1:
                return new RenoDXModMatch(database.GameMods[gameModCandidates[0]], null, null,
                    RenoDXModMatch.MatchKind.Fuzzy, []);
            case > 1:
                return CreateTie([.. gameModCandidates.Select(index => database.GameMods[index].Name)]);
        }

        var unrealModCandidates = FindClosestFuzzyMatches(preparedGameName, unrealModNames);

        switch (unrealModCandidates.Length)
        {
            case 1:
                return new RenoDXModMatch(null, database.UnrealGenericMods[unrealModCandidates[0]], null,
                    RenoDXModMatch.MatchKind.Fuzzy, []);
            case > 1:
                return CreateTie([.. unrealModCandidates.Select(index => database.UnrealGenericMods[index].Name)]);
        }

        var unityModCandidates = FindClosestFuzzyMatches(preparedGameName, unityModNames);

        return unityModCandidates.Length switch
        {
            1 => new RenoDXModMatch(null, null, database.UnityGenericMods[unityModCandidates[0]],
                RenoDXModMatch.MatchKind.Fuzzy, []),
            > 1 => CreateTie([.. unityModCandidates.Select(index => database.UnityGenericMods[index].Name)]),
            _ => RenoDXModMatch.None
        };
    }

    private static int? FindExactMatch(string preparedGameName, ImmutableArray<string> preparedModNames)
    {
        var normalizedModNames = preparedModNames.Select(GameNameHelper.NormalizeName)
            .ToImmutableArray();

        var index = normalizedModNames.IndexOf(GameNameHelper.NormalizeName(preparedGameName));

        return index == -1 ? null : index;
    }

    private static ImmutableArray<int> FindClosestFuzzyMatches(string preparedGameName,
        ImmutableArray<string> preparedModNames)
    {
        var closestMatches = ImmutableArray.CreateBuilder<int>();
        var highestOverlap = 0;

        for (var index = 0; index < preparedModNames.Length; index++)
        {
            if (!GameNameHelper.IsLikelySameGame(preparedGameName, preparedModNames[index]))
                continue;

            var overlap = GameNameHelper.CountSharedWords(preparedGameName, preparedModNames[index]);

            if (overlap > highestOverlap)
            {
                highestOverlap = overlap;
                closestMatches.Clear();
            }

            if (overlap == highestOverlap)
                closestMatches.Add(index);
        }

        return closestMatches.ToImmutable();
    }

    private static RenoDXModMatch CreateTie(ImmutableArray<string> tiedNames) =>
        new(null, null, null, RenoDXModMatch.MatchKind.Tie, tiedNames);

    private static ImmutableArray<string> PrepareNames(IEnumerable<string> names) =>
        [.. names.Select(PrepareName)];

    private static string PrepareName(string name) =>
        GameNameHelper.SplitInWordSeparators(GameNameHelper.RemoveLatinAccents(name));
}
