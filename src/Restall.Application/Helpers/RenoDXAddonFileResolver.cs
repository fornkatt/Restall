// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using System.Collections.Immutable;

namespace Restall.Application.Helpers;

public static class RenoDXAddonFileResolver
{
    public static RenoDXAddonFile? Resolve(string addonFilename, Uri? directUrl, RenoDXTagInfo? snapshot,
        ImmutableArray<RenoDXTagInfo> nightlies)
    {
        var addonFile = new RenoDXAddonFile(
            addonFilename,
            directUrl,
            FindSnapshot(addonFilename, snapshot),
            [.. nightlies.Where(nightly => nightly.AddonFilenames.Contains(addonFilename))]);

        return addonFile.Branches.IsEmpty ? null : addonFile;
    }

    private static RenoDXTagInfo? FindSnapshot(string addonFilename, RenoDXTagInfo? snapshot) => snapshot switch
    {
        null => null,
        { AddonFilenames.Count: 0 } => snapshot,
        _ when snapshot.AddonFilenames.Contains(addonFilename) => snapshot,
        _ => null
    };
}
