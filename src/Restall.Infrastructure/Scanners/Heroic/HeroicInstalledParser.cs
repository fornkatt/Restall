// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Helpers;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Scanners.Heroic;

internal static class HeroicInstalledParser
{
    internal static List<HeroicInstalledGame> EpicHeroicParser(string installedJson)
    {
        var games = new List<HeroicInstalledGame>();

        foreach (Match match in RegexHelper.HeroicGameBlockRegex.Matches(installedJson))
        {
            var blockValue = match.Value;

            var appName = RegexHelper.EpicHeroicAppNameRegex.Match(blockValue)
                is { Success: true } am
                ? am.Groups[1].Value
                : null;

            var installPath = RegexHelper.HeroicInstallPathRegex.Match(blockValue)
                is { Success: true } pm
                ? pm.Groups[1].Value.Replace("\\\\", "\\")
                : null;

            installPath = GameScanHelper.NormalizePath(installPath);

            var isDlc = Regex.IsMatch(blockValue, @"""is_dlc""\s*:\s*true", RegexOptions.IgnoreCase);

            games.Add(new HeroicInstalledGame(appName, installPath, isDlc));
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

            games.Add(new HeroicInstalledGame(appName, installPath, isDlc));
        }

        return games;
    }
}
