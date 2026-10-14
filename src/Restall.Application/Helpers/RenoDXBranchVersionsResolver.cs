// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Entities;
using System.Collections.Frozen;
using System.Diagnostics;

namespace Restall.Application.Helpers;

public static class RenoDXBranchVersionsResolver
{
    public static FrozenDictionary<RenoDX.Branch, UpdateAvailability> Resolve(RenoDX? installedRenoDX,
        RenoDXDownloadOptions? downloadOptions)
    {
        if (downloadOptions is null)
            return FrozenDictionary<RenoDX.Branch, UpdateAvailability>.Empty;

        return downloadOptions.Branches.ToFrozenDictionary(branch => branch,
            branch => CreateUpdateAvailability(branch, installedRenoDX, downloadOptions));
    }

    private static UpdateAvailability CreateUpdateAvailability(RenoDX.Branch branch, RenoDX? installedRenoDX,
        RenoDXDownloadOptions downloadOptions) => branch switch
        {
            RenoDX.Branch.Snapshot => CompareWithLatest(installedRenoDX, downloadOptions.Snapshot!),
            RenoDX.Branch.Nightly => CompareWithLatest(installedRenoDX,
                downloadOptions.LatestNightly!),
            RenoDX.Branch.Direct => new UpdateAvailability(false, null, false),
            _ => throw new UnreachableException($"No version source for branch \"{branch}\"")
        };

    private static UpdateAvailability CompareWithLatest(RenoDX? installedFile, RenoDXTagInfo latest) =>
        new(installedFile?.BuildDate < latest.Date, latest.Version);
}
