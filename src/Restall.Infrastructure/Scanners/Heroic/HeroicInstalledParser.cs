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

            var installPath = GameScanHelper.NormalizePath(entry.Value.TryGetProperty("install_path", out var installPathElement) ? installPathElement.GetString() : null);

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


        foreach (Match match in RegexHelper.HeroicGameBlockRegex.Matches(installedJson))
        {
            var blockValue = match.Value;

            var appName = RegexHelper.GOGHeroicAppNameRegex.Match(blockValue)
                is { Success: true } am
                ? am.Groups[1].Value
                : null;

            var installPath = RegexHelper.HeroicInstallPathRegex.Match(blockValue)
                is { Success: true } pm
                ? pm.Groups[1].Value.Replace("\\\\", "\\")
                : null;
            installPath = GameScanHelper.NormalizePath(installPath);

            var isDlc = Regex.IsMatch(blockValue, @"""is_dlc""\s*:\s*true", RegexOptions.IgnoreCase);

            games.Add(new HeroicInstalledGame(appName, installPath, isDlc, null));
        }

        return games;
    }
}
