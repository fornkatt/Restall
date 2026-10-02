// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.Json;

namespace Restall.Infrastructure.Scanners.Heroic;

internal static class HeroicInstalledParser
{
    internal static List<HeroicInstalledGame> EpicHeroicParser(string installedJson)
    {
        var games = new List<HeroicInstalledGame>();
        using var doc = JsonDocument.Parse(installedJson);

        foreach(var entry in doc.RootElement.EnumerateObject())
        {
            var appName = entry.Value.TryGetProperty("app_name", out var appNameElement) &&
                          appNameElement.ValueKind == JsonValueKind.String
                ? appNameElement.GetString() ?? string.Empty
                : string.Empty;

            var installPath = entry.Value.TryGetProperty("install_path", out var installPathElement) &&
                              installPathElement.ValueKind == JsonValueKind.String
                              ? installPathElement.GetString() ?? string.Empty
                              : string.Empty;

            var isDlc = entry.Value.TryGetProperty("is_dlc", out var isDlcElement) &&
                        isDlcElement.ValueKind == JsonValueKind.True;


            games.Add(new HeroicInstalledGame(appName, installPath, isDlc));
        }

        return games;
    }

    internal static List<HeroicInstalledGame> GOGHeroicParser(string installedJson)
    {
        var games = new List<HeroicInstalledGame>();
        using var doc = JsonDocument.Parse(installedJson);

        if (!doc.RootElement.TryGetProperty("installed", out var installedArray)) return games;

        foreach (var entry in installedArray.EnumerateArray())
        {
            var appName = entry.TryGetProperty("appName", out var appNameElement) &&
                          appNameElement.ValueKind == JsonValueKind.String
                ? appNameElement.GetString() ?? string.Empty
                : string.Empty;

            var installPath = entry.TryGetProperty("install_path", out var installPathElement) &&
                              installPathElement.ValueKind == JsonValueKind.String
                ? installPathElement.GetString() ?? string.Empty
                : string.Empty;

            var isDlc = entry.TryGetProperty("is_dlc", out var isDlcElement) &&
                        isDlcElement.ValueKind == JsonValueKind.True;

            games.Add(new HeroicInstalledGame(appName, installPath, isDlc));
        }

        return games;
    }
}
