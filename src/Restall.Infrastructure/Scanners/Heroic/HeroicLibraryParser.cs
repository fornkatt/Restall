// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Helpers;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Scanners.Heroic;

internal static class HeroicLibraryParser
{
    internal static List<HeroicLibrary> GOGLibraryParser(string libraryJson)
    {
        var libraries = new List<HeroicLibrary>();

        foreach (Match match in RegexHelper.InstallInfoAppNameAndTitleRegex.Matches(libraryJson))
        {
            var appName = match.Groups[1].Value;
            var title = match.Groups[2].Value;

            libraries.Add(new HeroicLibrary(appName, title));
        }

        return libraries;
    }
}
