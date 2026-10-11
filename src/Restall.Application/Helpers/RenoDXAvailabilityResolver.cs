// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Application.Helpers;

public static class RenoDXAvailabilityResolver
{
    public static RenoDXAvailability Resolve(RenoDX? installedRenoDX, Game.Engine engine, Architecture architecture,
        RenoDXModMatch match, bool isGameModListLoaded, RenoDXTagInfo? snapshot,
        ImmutableArray<RenoDXTagInfo> nightlies)
    {
        var notices = ImmutableArray.CreateBuilder<RenoDXAvailability.Notice>();

        if (match.Kind == RenoDXModMatch.MatchKind.Tie)
            notices.Add(RenoDXAvailability.Notice.AmbiguousMatch);
        if (installedRenoDX is not null && installedRenoDX.Arch != architecture)
            notices.Add(RenoDXAvailability.Notice.InstalledArchitectureMismatch);

        var installedAddonFilename = installedRenoDX?.Arch == architecture ? installedRenoDX.OriginalName : null;
        var engineFallbackAddonFilename = match.Kind == RenoDXModMatch.MatchKind.None && isGameModListLoaded
            ? GetEngineFallbackAddonFilename(engine, architecture)
            : null;
        var addonFilename = installedAddonFilename
                            ?? match.GetAddonFilename(architecture)
                            ?? engineFallbackAddonFilename;
        var isSupported = installedRenoDX is not null || HasMod(match) || engineFallbackAddonFilename is not null;

        if (!isSupported)
            return RenoDXAvailability.NotSupported with { Notices = notices.ToImmutable() };
        if (installedAddonFilename is null && engineFallbackAddonFilename is not null)
            notices.Add(RenoDXAvailability.Notice.EngineFallback);

        var gameModAddonFilename = match.GameMod?.GetAddonFilename(architecture);
        var isGameModFile = gameModAddonFilename?.Equals(addonFilename, StringComparison.OrdinalIgnoreCase) == true;
        var genericAddonInfo = addonFilename is null || isGameModFile
            ? null
            : RenoDXGenericAddons.GetGenericAddonInfo(addonFilename);

        if (genericAddonInfo is not null && gameModAddonFilename is not null)
            notices.Add(RenoDXAvailability.Notice.GameSpecificModAvailable);

        var downloadOptions = addonFilename is null
            ? null
            : RenoDXDownloadOptionsResolver.Resolve(addonFilename, GetDirectUrl(addonFilename, match.GameMod),
                snapshot, nightlies, installedAddonFilename is not null);

        if (downloadOptions?.Snapshot is { HasAddonFileList: false })
            notices.Add(RenoDXAvailability.Notice.SnapshotFileUnconfirmed);

        var hasBuildForOtherArchitecture = addonFilename is null && HasBuildForOtherArchitecture(match, architecture);

        if (hasBuildForOtherArchitecture)
            notices.Add(RenoDXAvailability.Notice.NoBuildForArchitecture);

        var manualSource =
            downloadOptions is null
            && !hasBuildForOtherArchitecture
            && match.GameMod is { } gameMod
                ? RenoDXModLinkBuilder.CreateModPageLink(gameMod.NexusUrl)
                  ?? RenoDXModLinkBuilder.CreateDiscordLink(gameMod.DiscordUrl)
                : null;

        if (downloadOptions is null && manualSource is null && !hasBuildForOtherArchitecture)
            notices.Add(RenoDXAvailability.Notice.NoDownloadListed);

        return new RenoDXAvailability(true, downloadOptions, manualSource, notices.ToImmutable(),
            genericAddonInfo);
    }

    private static string? GetEngineFallbackAddonFilename(Game.Engine engine, Architecture architecture) =>
        engine switch
        {
            Game.Engine.Unreal => RenoDXGenericAddons.GetUnrealExtendedAddonFilename(architecture),
            Game.Engine.Unity => RenoDXGenericAddons.GetUnityGenericAddonFilename(architecture),
            _ => null
        };

    private static bool HasMod(RenoDXModMatch match) =>
        match.GameMod is not null || match.UnrealGenericMod is not null || match.UnityGenericMod is not null;

    private static bool HasBuildForOtherArchitecture(RenoDXModMatch match, Architecture architecture)
    {
        var otherArchitecture = architecture == Architecture.X32 ? Architecture.X64 : Architecture.X32;

        return match.GetAddonFilename(otherArchitecture) is not null;
    }

    private static Uri? GetDirectUrl(string addonFilename, RenoDXGameMod? gameMod) =>
        RenoDXGenericAddons.GetGenericAddonDownloadUrl(addonFilename) ?? gameMod?.GetDownloadUrl(addonFilename);
}
