// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;

namespace Restall.Application.Helpers;

public static class RenoDXModLinkBuilder
{
    public static RenoDXModLink? CreateModPageLink(string? url) =>
        TryCreateHttpsUri(url) is { } uri
            ? new RenoDXModLink(GetModPageLabel(uri), uri)
            : null;

    public static RenoDXModLink? CreateDiscordLink(string? url) =>
        TryCreateHttpsUri(url) is { } uri
            ? new RenoDXModLink("Discord", uri)
            : null;

    public static RenoDXModLink? CreateGitHubDiscussionLink(string? url) =>
        TryCreateHttpsUri(url) is { } uri
            ? new RenoDXModLink("GitHub discussion", uri)
            : null;

    private static string GetModPageLabel(Uri uri) => uri.Host.ToLowerInvariant() switch
    {
        "nexusmods.com" or "www.nexusmods.com" => "Nexus Mods",
        "gamebanana.com" or "www.gamebanana.com" => "GameBanana",
        var host when host.StartsWith("www.") => host["www.".Length..],
        var host => host
    };

    private static Uri? TryCreateHttpsUri(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps ? uri : null;
}
