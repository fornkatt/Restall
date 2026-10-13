// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXTagInfo(
    DateOnly Date,
    Uri DownloadBaseUrl,
    FrozenSet<string> AddonFilenames,
    ImmutableArray<string> CommitNotes)
{
    public string Version => Date.ToString(RenoDX.VersionFormat, CultureInfo.InvariantCulture);

    public bool HasAddonFileList => AddonFilenames.Count > 0;

    public Uri GetDownloadUrl(string addonFilename) =>
        new(DownloadBaseUrl, Uri.EscapeDataString(addonFilename));
}
