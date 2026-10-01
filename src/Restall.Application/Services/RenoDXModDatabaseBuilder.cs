// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Restall.Application.Services;

public sealed partial class RenoDXModDatabaseBuilder
{
    private readonly ILogger<RenoDXModDatabaseBuilder> _logger;

    public RenoDXModDatabaseBuilder(
        ILogger<RenoDXModDatabaseBuilder> logger
    )
    {
        _logger = logger;
    }

    public Result<RenoDXModDatabase> Build(
        ImmutableArray<RenoDXGameMod> gameMods,
        ImmutableArray<RenoDXUnrealGenericMod> unrealGenericMods,
        ImmutableArray<RenoDXUnityGenericMod> unityGenericMods)
    {
        LogRenoDXDatabaseBuildStart();

        var database = new RenoDXModDatabase(
            KeepUsableEntries(gameMods),
            KeepUsableEntries(unrealGenericMods),
            KeepUsableEntries(unityGenericMods));

        var gameModsDroppedCount = gameMods.Length - database.GameMods.Length;
        var unrealModsDroppedCount = unrealGenericMods.Length - database.UnrealGenericMods.Length;
        var unityModsDroppedCount = unityGenericMods.Length - database.UnityGenericMods.Length;
        var totalDroppedCount = gameModsDroppedCount + unrealModsDroppedCount + unityModsDroppedCount;

        LogRenoDXDatabaseBuildComplete(database.GameMods.Length, database.UnrealGenericMods.Length,
            database.UnityGenericMods.Length);

        if (database.GameMods.IsEmpty && database.UnrealGenericMods.IsEmpty && database.UnityGenericMods.IsEmpty)
            return Result<RenoDXModDatabase>.Error("No 'RenoDX' mods available from database files",
                ErrorType.RenoDXModDatabaseUnavailable);

        if (totalDroppedCount > 0
            || database.GameMods.IsEmpty
            || database.UnrealGenericMods.IsEmpty
            || database.UnityGenericMods.IsEmpty)
            return Result<RenoDXModDatabase>.Partial(database,
                $"Some entries were dropped from the RenoDX mod database. Game mods: {gameModsDroppedCount}, " +
                $"Unreal generic mods: {unrealModsDroppedCount}, Unity generic mods: {unityModsDroppedCount}",
                WarningType.RenoDXModDatabaseIncomplete);

        return Result<RenoDXModDatabase>.Success(database);
    }

    private ImmutableArray<T> KeepUsableEntries<T>(ImmutableArray<T> entries) where T : class
    {
        var usableEntries = ImmutableArray.CreateBuilder<T>(entries.Length);

        foreach (var entry in entries)
        {
            var dropReason = GetDropReason(entry);

            if (dropReason is null)
                usableEntries.Add(entry);
            else
                LogRenoDXDatabaseEntryValidationFailure(dropReason, $"{entry}");
        }

        return usableEntries.DrainToImmutable();
    }

    private static string? GetDropReason(object entry) => entry switch
    {
        RenoDXGameMod mod => GetGameModDropReason(mod.Name, mod.Status),
        RenoDXUnrealGenericMod mod => GetUnrealGenericModDropReason(mod),
        RenoDXUnityGenericMod mod => GetUnityGenericModDropReason(mod.Name, mod.Status),
        _ => throw new UnreachableException($"No drop rules for \"{entry.GetType().Name}\"")
    };

    private static string? GetGameModDropReason(string name, RenoDXModStatus status) =>
        GetNameOrStatusDropReason(name, status);

    private static string? GetUnrealGenericModDropReason(RenoDXUnrealGenericMod mod)
    {
        var nameOrStatusDropReason = GetNameOrStatusDropReason(mod.Name, mod.Status);

        if (nameOrStatusDropReason is not null)
            return nameOrStatusDropReason;

        if (mod.Method is not (RenoDXUnrealGenericMod.UnrealModMethod.Native
            or RenoDXUnrealGenericMod.UnrealModMethod.Upgrade
            or RenoDXUnrealGenericMod.UnrealModMethod.Ini))
            return $"Unknown Unreal mod method \"{mod.Method}\"";

        return null;
    }

    private static string? GetUnityGenericModDropReason(string name, RenoDXModStatus status) =>
        GetNameOrStatusDropReason(name, status);

    private static string? GetNameOrStatusDropReason(string name, RenoDXModStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Missing name";

        if (status is not (RenoDXModStatus.Done or RenoDXModStatus.Wip))
            return $"Unknown status \"{status}\"";

        return null;
    }
}
