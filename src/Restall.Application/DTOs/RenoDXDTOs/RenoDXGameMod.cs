// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXGameMod(
    string Name,
    RenoDXModStatus Status,
    string? Author,
    string? SnapshotUrl,
    string? SnapshotUrl32,
    string? NexusUrl,
    string? DiscordUrl,
    string? DiscussionUrl,
    string? Notes)
{
    public string? GetAddonFilename(Architecture architecture) =>
        architecture == Architecture.X32 ? AddonFilename32 : AddonFilename;

    public Uri? GetDownloadUrl(string addonFilename)
    {
        if (string.Equals(addonFilename, AddonFilename, StringComparison.OrdinalIgnoreCase))
            return new Uri(SnapshotUrl!);
        if (string.Equals(addonFilename, AddonFilename32, StringComparison.OrdinalIgnoreCase))
            return new Uri(SnapshotUrl32!);

        return null;
    }

    private string? AddonFilename => ExtractAddonFilename(SnapshotUrl, RenoDX.AddonExtension);
    private string? AddonFilename32 => ExtractAddonFilename(SnapshotUrl32, RenoDX.AddonExtension32);

    private static string? ExtractAddonFilename(string? url, string extension)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            return null;

        var filename = Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath));

        return filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? filename : null;
    }
}
