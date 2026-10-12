// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Entities;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Restall.Application.Helpers;

public static class RenoDXReleaseNotesResolver
{
    private const int MaxNightlyReleases = 10;

    public static FrozenDictionary<RenoDX.Branch, ImmutableArray<RenoDXReleaseNotes>> Resolve(RenoDX? installedRenoDX,
        RenoDXDownloadOptions? downloadOptions)
    {
        if (downloadOptions is null)
            return FrozenDictionary<RenoDX.Branch, ImmutableArray<RenoDXReleaseNotes>>.Empty;

        var installedFile = downloadOptions.IsInstalledFile ? installedRenoDX : null;

        return downloadOptions.Branches.ToFrozenDictionary(b => b,
            b => GetReleaseNotes(b, installedFile, downloadOptions));
    }

    private static ImmutableArray<RenoDXReleaseNotes> GetReleaseNotes(RenoDX.Branch branch, RenoDX? installedRenoDX,
        RenoDXDownloadOptions downloadOptions) => branch switch
    {
        RenoDX.Branch.Snapshot =>
            [CreateReleaseNotes(downloadOptions.Snapshot!, installedRenoDX, downloadOptions.Snapshot!.Date)],
        RenoDX.Branch.Nightly => GetNightlyReleaseNotes(installedRenoDX, downloadOptions.Nightlies),
        RenoDX.Branch.Direct => [],
        _ => throw new UnreachableException($"No release notes source for branch \"{branch}\"")
    };

    private static ImmutableArray<RenoDXReleaseNotes> GetNightlyReleaseNotes(RenoDX? installedRenoDX,
        ImmutableArray<RenoDXTagInfo> nightlies)
    {
        var newestFirst = nightlies.OrderByDescending(n => n.Date).ToList();
        var latestDate = newestFirst[0].Date;
        var releases = installedRenoDX?.BuildDate is { } installedDate && installedDate <= latestDate
            ? newestFirst.TakeWhile(n => n.Date >= installedDate)
            : newestFirst.Take(1);

        return
        [
            .. releases.Take(MaxNightlyReleases).Select(n => CreateReleaseNotes(n, installedRenoDX, latestDate))
        ];
    }

    private static RenoDXReleaseNotes CreateReleaseNotes(RenoDXTagInfo release, RenoDX? installedFile,
        DateOnly latestDate) =>
        new(release.Version,
            installedFile?.BuildDate == release.Date,
            release.Date == latestDate,
            release.CommitNotes);
}
