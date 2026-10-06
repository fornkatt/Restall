// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;
using Restall.Infrastructure.DTOs.Heroic;
using Restall.Infrastructure.Helpers;
using System.Text.Json;

namespace Restall.Infrastructure.Scanners.Heroic;

internal static class HeroicInstalledParser
{
    internal static List<HeroicInstalledGame> InstalledParser(string installedJson, Game.Platform platform)
    {
        using var doc = JsonDocument.Parse(installedJson);
        var root = doc.RootElement;

        return platform switch
        {
            Game.Platform.Epic => ReadEpicInstalled(root),
            Game.Platform.GOG => ReadGOGInstalled(root),
            _ => throw new ArgumentOutOfRangeException(nameof(platform), "Unsupported Platform")
        };

    }

    private static List<HeroicInstalledGame> ReadEpicInstalled(JsonElement root)
    {
        var games = new List<HeroicInstalledGame>();
        foreach (var prop in root.EnumerateObject())
        {
            games.Add(ReadEntry(prop.Value, "app_name"));
        }
        return games;
    }

    private static List<HeroicInstalledGame> ReadGOGInstalled(JsonElement root)
    {
        var games = new List<HeroicInstalledGame>();
        foreach(var entry in root.GetProperty("installed").EnumerateArray())
        {
            games.Add(ReadEntry(entry, "appName"));
        }
        return games;
    }

    private static HeroicInstalledGame ReadEntry(JsonElement entry, string appName) =>
        new(
            GameScanHelper.ReadJsonString(entry, appName),
            GameScanHelper.ReadJsonString(entry, "install_path"),
            entry.TryGetProperty("is_dlc", out var isDlcElement)
            && isDlcElement.ValueKind == JsonValueKind.True
        );

}
