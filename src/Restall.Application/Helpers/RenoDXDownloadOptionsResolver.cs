// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using System.Collections.Immutable;

namespace Restall.Application.Helpers;

public static class RenoDXDownloadOptionsResolver
{
    public static RenoDXDownloadOptions? Resolve(string addonFilename, Uri? directUrl, RenoDXTagInfo? snapshot,
        ImmutableArray<RenoDXTagInfo> nightlies, bool isInstalledFile = false)
    {
        var downloadOptions = new RenoDXDownloadOptions(
            addonFilename,
            directUrl,
            FindSnapshot(addonFilename, snapshot),
            [.. nightlies.Where(nightly => nightly.AddonFilenames.Contains(addonFilename))],
            isInstalledFile);

        return downloadOptions.Branches.IsEmpty ? null : downloadOptions;
    }

    private static RenoDXTagInfo? FindSnapshot(string addonFilename, RenoDXTagInfo? snapshot) => snapshot switch
    {
        null => null,
        { HasAddonFileList: false } => snapshot,
        _ when snapshot.AddonFilenames.Contains(addonFilename) => snapshot,
        _ => null
    };
}
