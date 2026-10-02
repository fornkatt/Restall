// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Text.Json;


namespace Restall.Infrastructure.Scanners.Heroic;

internal static class HeroicLibraryParser
{
    internal static List<HeroicLibrary> GOGLibraryParser(string libraryJson)
    {
        var libraries = new List<HeroicLibrary>();
        using var doc = JsonDocument.Parse(libraryJson);

        if (!doc.RootElement.TryGetProperty("games", out var gamesElement)) return libraries;

        foreach (var entry in gamesElement.EnumerateArray())
        {
            var appName = entry.TryGetProperty("app_name", out var appNameElement) &&
                          appNameElement.ValueKind == JsonValueKind.String ? appNameElement.GetString()
                                                                             ?? string.Empty : string.Empty;

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
        using var doc = JsonDocument.Parse(libraryJson);

        if (!doc.RootElement.TryGetProperty("library", out var libraryElement)) return libraries;

        foreach (var entry in libraryElement.EnumerateArray())
        {
            var appName = entry.TryGetProperty("app_name", out var appNameElement) &&
                          appNameElement.ValueKind == JsonValueKind.String ? appNameElement.GetString()
                                                                             ?? string.Empty : string.Empty;

            var title = entry.TryGetProperty("title", out var titleElement) &&
                        titleElement.ValueKind == JsonValueKind.String
                ? titleElement.GetString() ?? string.Empty
                : string.Empty;

            libraries.Add(new HeroicLibrary(appName, title));
        }

        return libraries;
    }

}
