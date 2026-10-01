// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Helpers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Scanners.Heroic;

internal static class HeroicInstalledParser
{
    internal static List<HeroicInstalledGame> EpicHeroicParser(string installedJson)
    {
        var games = new List<HeroicInstalledGame>();
        using var doc = JsonDocument.Parse(installedJson);

        foreach(var entry in doc.RootElement.EnumerateObject())
        {
            var appName = entry.Value.TryGetProperty("app_name", out var appNameElement)
                ? appNameElement.GetString() : null;

            var installPath = GameScanHelper.NormalizePath(entry.Value.TryGetProperty("install_path", out var installPathElement)
                ? installPathElement.GetString() : null);

            var isDlc = entry.Value.TryGetProperty("is_dlc", out var isDlcElement) && isDlcElement.GetBoolean();

            var title = entry.Value.TryGetProperty("title", out var titleElement)
                ? titleElement.GetString() : null;

            games.Add(new HeroicInstalledGame(appName, installPath, isDlc, title));
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
            var appName = entry.TryGetProperty("appName", out var appNameElement)
                ? appNameElement.GetString() : null;

            var installPath = GameScanHelper.NormalizePath(entry.TryGetProperty("install_path", out var installPathElement)
                ? installPathElement.GetString() : null);

            var isDlc = entry.TryGetProperty("is_dlc", out var isDlcElement) && isDlcElement.GetBoolean();

            games.Add(new HeroicInstalledGame(appName, installPath, isDlc, null));
        }

        return games;
    }
}
