// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;

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
    public string? AddonFilename => GetAddonFilename(SnapshotUrl, ".addon64");
    public string? AddonFilename32 => GetAddonFilename(SnapshotUrl32, ".addon32");

    public Uri? GetDownloadUrl(string addonFilename)
    {
        if (string.Equals(addonFilename, AddonFilename, StringComparison.OrdinalIgnoreCase))
            return new Uri(SnapshotUrl!);
        if (string.Equals(addonFilename, AddonFilename32, StringComparison.OrdinalIgnoreCase))
            return new Uri(SnapshotUrl32!);

        return null;
    }

    private static string? GetAddonFilename(string? url, string extension)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            return null;

        var filename = Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath));

        return filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? filename : null;
    }
}
