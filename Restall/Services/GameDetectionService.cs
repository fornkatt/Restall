using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Restall.Models;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Restall.Helpers;

namespace Restall.Services;

/// <summary>
/// EXECUTABLE PATH
/// Epic - GOG - Ubisoft - EA AND REWORK THEM
/// Heroic (Most likely the trickiest beacuse we want to exclude the libraries they come from
/// STEAMGRIDDBID
/// </summary>
public class GameDetectionService : IGameDetectionService
{
    public async Task<List<Game?>> FindGames()
    {
        try
        {
            var tasks = new[]
            {
                FindSteamGamesAsync(),
            };
            var results = await Task.WhenAll(tasks);
            var allGames = results.SelectMany(t => t).ToList();
            var sortGames = allGames.GroupBy(g => g.Name)
                .Select(g => g.FirstOrDefault());
            var path = sortGames.GroupBy(g => g.InstallFolder).Select(g => g.OrderBy(g => g.Name).First());
            return path.ToList();
        }
        catch
        {
            Debug.WriteLine("Couldn't find any games...");
            return new List<Game?>();
        }
    }

    //TODO: INCLUDE OTHERS

    #region Asyncronous Methods

    private Task<List<Game>> FindSteamGamesAsync() => Task.Run(FindSteamGames);

    #endregion


    #region STEAM

    private List<Game> FindSteamGames()
    {
        var games = new List<Game>();
        var steamPath = GetSteamInstallPath();

        if (steamPath == null) return games;
        steamPath = Helper.NormalizePath(steamPath);
        foreach (var library in GetSteamLibraries(steamPath))
        {
            games.AddRange(ScanSteamLibrary(library));
        }

        return games;
    }

    private string? GetSteamInstallPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return Helper.ReadRegistry(@"SOFTWARE\Valve\Steam", "SteamPath")
                   ?? Helper.ReadRegistry(@"SOFTWARE\WOW6432Node\Valve\Steam", "SteamPath");
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            var linuxPaths = new[]
            {
                Path.Combine(home, ".steam", "steam"),
                Path.Combine(home, ".local", "share", "Steam"),
            };

            foreach (var path in linuxPaths)
            {
                if (Directory.Exists(path)) return path;
            }

            return null;
        }
    }

    private List<Game> ScanSteamLibrary(string library)
    {
        var games = new List<Game>();
        var steamapps = Path.Combine(library, "steamapps");
        if (!Directory.Exists(steamapps)) return games;
        foreach (var acf in Directory.GetFiles(steamapps, "appmanifest_*.acf"))
        {
            try
            {
                var content = File.ReadAllText(acf);
                var name = Helper.ExtractVdfValue(content, "name");
                var installDir = Helper.ExtractVdfValue(content, "installdir");
                if (name == null || installDir == null) continue;

                var rootPath = Path.Combine(steamapps, "common", installDir);
                if (!Directory.Exists(rootPath)) continue;

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = rootPath,

                    PlatformName = Game.Platform.Steam,
                });
            }
            catch { }
        }

        return games;
    }
    
    private List<string> GetSteamLibraries(string path)
    {
        var libraries = new List<string>();
        var vdfPath = Path.Combine(path, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdfPath)) return libraries;
        foreach (Match match in Regex.Matches(File.ReadAllText(vdfPath), @"""path""\s+""([^""]+)"""))
        {
            var library = Helper.NormalizePath(match.Groups[1].Value);
            if (Directory.Exists(library) && !libraries.Contains(library))
            {
                libraries.Add(library);
            }
        }

        return libraries;
    }

    #endregion
}