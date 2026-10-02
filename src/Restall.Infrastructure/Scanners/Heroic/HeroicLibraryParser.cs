// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Helpers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Scanners.Heroic;

internal static class HeroicLibraryParser
{
    internal static List<HeroicLibrary> GOGLibraryParser(string libraryJson)
    {
        var libraries = new List<HeroicLibrary>();
        using var doc = JsonDocument.Parse(libraryJson);

        if (!doc.RootElement.TryGetProperty("games", out var gamesArray)) return libraries;

        foreach (var entry in gamesArray.EnumerateArray())
        {
            var appName = entry.TryGetProperty("app_name", out var appNameElement) &&
                          appNameElement.ValueKind == JsonValueKind.String ? appNameElement.GetString() ?? string.Empty : string.Empty;

            var title = entry.TryGetProperty("title", out var titleElement) &&
                        titleElement.ValueKind == JsonValueKind.String
                ? titleElement.GetString() ?? string.Empty
                : string.Empty;

            libraries.Add(new HeroicLibrary(appName, title));
        }

        return libraries;
    }

    internal static List<HeroicLibrary> EpicLibraryParser(string libraryJson)
    {
        var libraries = new List<HeroicLibrary>();
        return libraries;
    }

}
