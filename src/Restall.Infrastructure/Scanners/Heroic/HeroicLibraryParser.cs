// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later


using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Text.Json;


namespace Restall.Infrastructure.Scanners.Heroic;

internal static class HeroicLibraryParser
{
    internal static List<HeroicLibrary> LibraryParser(string libraryJson, Game.Platform platform)
    {
        var libraries = new List<HeroicLibrary>();
        using var doc = JsonDocument.Parse(libraryJson);
        var arrayKey = platform switch
        {
            Game.Platform.Epic => "library",
            Game.Platform.GOG => "games",
            _ => throw new ArgumentOutOfRangeException(nameof(platform), "Unsupported Platform")
        };

        foreach (var entry in doc.RootElement.GetProperty(arrayKey).EnumerateArray())
        {
            var appName = GameScanHelper.ReadJsonString(entry, "app_name");

            var title = GameScanHelper.ReadJsonString(entry, "title");

            libraries.Add(new HeroicLibrary(appName, title));
        }

        return libraries;
    }


}
