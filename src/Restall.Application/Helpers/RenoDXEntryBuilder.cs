// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Restall.Application.Helpers;

public static partial class RenoDXEntryBuilder
{
    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex UpgradeTokenRegex();

    public static RenoDXEntry Build(RenoDX? installedRenoDX, Game.Engine engine, Architecture architecture,
        RenoDXModMatch match, bool isGameModListLoaded, RenoDXTagInfo? snapshot,
        ImmutableArray<RenoDXTagInfo> nightlies)
    {
        var availability = RenoDXAvailabilityResolver.Resolve(installedRenoDX, engine, architecture, match,
            isGameModListLoaded, snapshot, nightlies);
        var installedFile = availability.DownloadOptions?.IsInstalledFile == true ? installedRenoDX : null;
        var branchVersions = RenoDXBranchVersionsResolver.Resolve(installedFile,
            availability.DownloadOptions);
        var branchReleaseNotes = RenoDXReleaseNotesResolver.Resolve(installedFile,
            availability.DownloadOptions);

        return new RenoDXEntry(
            IsSupported: availability.IsSupported,
            DownloadOptions: availability.DownloadOptions,
            ManualSource: availability.ManualSource,
            BranchVersions: branchVersions,
            BranchReleaseNotes: branchReleaseNotes,
            HeadingText: match.ModName is { } modName
                ? RenoDXTexts.GetListedAsText(modName)
                : availability.GenericAddonInfo?.HeadingText,
            StatusText: match.ModStatus is { } status ? RenoDXTexts.GetStatusText(status) : null,
            IsDone: match.ModStatus is RenoDXModStatus.Done,
            IsWorkInProgress: match.ModStatus is RenoDXModStatus.Wip,
            Author: match.GameMod?.Author,
            DatabaseNotes: match.GameMod?.Notes ?? match.UnrealGenericMod?.Comments ?? match.UnityGenericMod?.Comments,
            UnrealMethodText: match.UnrealGenericMod is { } unrealGenericMod
                ? RenoDXTexts.GetUnrealMethodText(unrealGenericMod.Method)
                : null,
            Upgrades: SplitUpgradeTokens(match.UnrealGenericMod?.Upgrades ?? match.UnityGenericMod?.Upgrades),
            Links: CreateLinks(match.GameMod),
            Notices: [.. availability.Notices.Select(RenoDXTexts.GetNoticeText)],
            GenericAddonInfo: availability.GenericAddonInfo);
    }

    private static ImmutableArray<string> SplitUpgradeTokens(string? upgrades)
    {
        if (string.IsNullOrWhiteSpace(upgrades))
            return [];

        ImmutableArray<string> tokens =
            [.. UpgradeTokenRegex().Matches(upgrades).Select(tokenMatch => tokenMatch.Groups[1].Value)];

        return tokens.IsEmpty ? [upgrades.Trim()] : tokens;
    }

    private static ImmutableArray<RenoDXModLink> CreateLinks(RenoDXGameMod? gameMod)
    {
        if (gameMod is null)
            return [];

        RenoDXModLink?[] links =
        [
            RenoDXModLinkBuilder.CreateModPageLink(gameMod.NexusUrl),
            RenoDXModLinkBuilder.CreateDiscordLink(gameMod.DiscordUrl),
            RenoDXModLinkBuilder.CreateGitHubDiscussionLink(gameMod.DiscussionUrl)
        ];

        return [.. links.OfType<RenoDXModLink>()];
    }
}
