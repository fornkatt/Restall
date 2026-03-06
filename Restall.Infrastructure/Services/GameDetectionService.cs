using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

/// <summary>
/// Heroic (Most likely the trickiest beacuse we want to exclude the libraries they come from
/// IGNORE DOTNET
/// ILAUNCHERDETECTION?
/// ADD HEALTH CHECK?
/// Ubisoft - EA AND REWORK THEM
/// STEAMGRIDDBID
/// </summary>
public class GameDetectionService : IGameDetectionService
{
    private readonly ILogService _logService;

    public GameDetectionService(
        ILogService logService
        )
    {
        _logService = logService;
    }

    public async Task<List<Game?>> FindGames()
    {
        try
        {
            var tasks = new[]
            {
                FindSteamGamesAsync(),
                FindEpicGamesAsync(),
                FindGOGGamesAsync()
            };
            var results = await Task.WhenAll(tasks);
            var allGames = results.SelectMany(t => t).ToList();


            foreach (var game in allGames)
            {
                await _logService.LogInfoAsync(
                    $"NAME: {game.Name} INSTALLFOLDER: {game.InstallFolder} PATH: {game.ExecutablePath} PLATFORM: {game.PlatformName} ENGINE: {game.EngineName} ");
            }

            var sortGames = allGames.GroupBy(g => g.Name)
                .Select(g => g.FirstOrDefault())
                .ToList();

            var path = sortGames.GroupBy(g => g.InstallFolder)
                .Select(g => g.OrderBy(g => g.Name).First())
                .ToList();


            return path;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Something went wrong with FindGames: {ex.Message}");
            return new List<Game?>();
        }
    }

    //TODO: INCLUDE OTHERS

    #region Asyncronous Methods

    private Task<List<Game>> FindSteamGamesAsync() => Task.Run(FindSteamGames);
    private Task<List<Game>> FindEpicGamesAsync() => Task.Run(FindEpicGames);

    private Task<List<Game>> FindGOGGamesAsync() => Task.Run(FindGOGGames);

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

                if (Helper.NonGame(name)) continue;

                var rootPath = Path.Combine(steamapps, "common", installDir);
                if (!Directory.Exists(rootPath)) continue;

                var executablePath = DetectExecutablePathAndEngine(rootPath, out var engine);

                if (string.IsNullOrEmpty(executablePath)) continue;

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = rootPath,
                    ExecutablePath = executablePath,
                    EngineName = engine,
                    PlatformName = Game.Platform.Steam
                });
            }
            catch
            {
                _logService.LogError("Could not find Steam library: " + library);
            }
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
            var library = Helper.NormalizePath(match.Groups[1].Value.Replace(@"\\", @"\"));
            if (Directory.Exists(library) && !libraries.Contains(library))
            {
                libraries.Add(library);
            }
        }

        return libraries;
    }

    #endregion

    #region EPIC

    private List<Game> FindEpicGames()
    {
        var games = new List<Game>();

        if (OperatingSystem.IsWindows())
        {
            var ueInstallPath = GetEpicInstallPath();


            if (ueInstallPath != null && Directory.Exists(ueInstallPath))
            {
                games.AddRange(ScanEpicLibrary(ueInstallPath));
            }
        }

        var epicHeroicPath = GetEpicHeroicPath();
        if (epicHeroicPath != null && Directory.Exists(epicHeroicPath))
        {
            games.AddRange(ScanEpicHeroicLibrary(epicHeroicPath));
        }

        return games;
    }

    private string? GetEpicInstallPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic", "EpicGamesLauncher", "Data", "Manifests");
    }

    private List<Game> ScanEpicLibrary(string manifestDir)
    {
        var games = new List<Game>();

        foreach (var file in Directory.GetFiles(manifestDir, "*.item"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var name = Helper.ExtractJsonString(json, "DisplayName");
                var rootPath = Helper.ExtractJsonString(json, "InstallLocation");

                if (rootPath != null)
                {
                    if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(rootPath))
                    {
                        _logService.LogWarning($" Skipping Epic: empty title or path");
                        continue;
                    }

                    if (!Directory.Exists(rootPath))
                    {
                        _logService.LogWarning($"Could not find install location: {rootPath}");
                        continue;
                    }

                    var executablePath = DetectExecutablePathAndEngine(rootPath, out var engine);
                    if (string.IsNullOrEmpty(executablePath))
                    {
                        _logService.LogWarning($"Could not find executable path: {rootPath}");
                        continue;
                    }

                    games.Add(new Game
                    {
                        Name = name,
                        InstallFolder = rootPath,
                        ExecutablePath = executablePath,
                        EngineName = engine,
                        PlatformName = Game.Platform.Epic
                    });
                }
            }
            catch
            {
                _logService.LogError("Could not find Epic library: " + file);
            }
        }

        return games;
    }

    #endregion

    #region GOG

    private List<Game> FindGOGGames()
    {
        var games = new List<Game>();

        if (OperatingSystem.IsWindows())
        {
            games.AddRange(ScanGOGLibrary());
        }

        var gogHeroicPath = GetGogHeroicPath();
        if (gogHeroicPath != null && Directory.Exists(gogHeroicPath))
        {
            games.AddRange(ScanGogHeroicLibrary(gogHeroicPath));
        }

        return games;
    }

    private List<Game> ScanGOGLibrary()
    {
        var games = new List<Game>();

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GOG.com\Games")
                            ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\GOG.com\Games");
            if (key == null) return games;

            foreach (var sub in key.GetSubKeyNames())
            {
                using var gameKey = key.OpenSubKey(sub);

                if (gameKey == null) continue;

                var name = gameKey.GetValue("GAMENAME") as string
                           ?? gameKey.GetValue("GameName") as string;

                var path = gameKey.GetValue("PATH") as string
                           ?? gameKey.GetValue("path") as string;
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
                    continue;
                if (!Directory.Exists(path))
                    continue;

                var executablePath = DetectExecutablePathAndEngine(path, out var engine);
                if (string.IsNullOrEmpty(executablePath)) continue;

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = path,
                    ExecutablePath = executablePath,
                    EngineName = engine,
                    PlatformName = Game.Platform.GOG
                });
            }
        }
        catch
        {
            _logService.LogError($"Could not find GOG games...{games}");
        }

        return games;
    }

    #endregion


    #region HEROIC

    private string? GetGogHeroicPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var heroicPath = OperatingSystem.IsWindows()
            ? Path.Combine(home, "AppData", "Roaming", "heroic", "gog_store", "installed.json")
            : Path.Combine(home, ".config", "heroic", "gog_store", "installed.json");

        return File.Exists(heroicPath) ? heroicPath : null;
    }

    private List<Game> ScanGogHeroicLibrary(string configDir)
    {
        throw new NotImplementedException();
    }


    private string? GetEpicHeroicPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var heroicPath = OperatingSystem.IsWindows()
            ? Path.Combine(home, "AppData", "Roaming", "heroic", "legendaryConfig", "legendary")
            : Path.Combine(home, ".config", "heroic", "legendaryConfig", "legendary");

        return Directory.Exists(heroicPath) ? heroicPath : null;
    }

    private List<Game> ScanEpicHeroicLibrary(string configDir)
    {
        var games = new List<Game>();
        var installedJsonPath = Path.Combine(configDir, "installed.json");


        if (!File.Exists(installedJsonPath))
        {
            _logService.LogWarning($"Could not find installed json file: {installedJsonPath}");
            return games;
        }

        try
        {
            var json = File.ReadAllText(installedJsonPath);

            /* Regex Pattern with Regex101
             *   ""title""\s*:\s*""([^""]+)""
             *   ""install_path""\s*:\s*""([^""]+)""
             */


            var titleRegexMatches = Regex.Matches(json, @"""title""\s*:\s*""([^""]+)""");
            var pathRegexMatches = Regex.Matches(json, @"""install_path""\s*:\s*""([^""]+)""");

            Debug.WriteLine($"[Heroic] Found {titleRegexMatches.Count} titles, {pathRegexMatches.Count} paths");

            for (int i = 0; i < Math.Min(titleRegexMatches.Count, pathRegexMatches.Count); i++)
            {
                var title = titleRegexMatches[i].Groups[1].Value;
                var installPath = pathRegexMatches[i].Groups[1].Value.Replace("\\\\", "\\");
                installPath = Helper.NormalizePath(installPath);


                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(installPath))
                {
                    _logService.LogWarning(
                        $"Could not find the install path nor title. Title: {title} InstallPath:{installPath}");
                    continue;
                }

                if (!Directory.Exists(installPath))
                {
                    _logService.LogWarning($"Install Path for Heroic {installPath} not found!");
                    continue;
                }

                var executablePath = DetectExecutablePathAndEngine(installPath, out var engine);

                if (string.IsNullOrEmpty(executablePath)) continue;

                games.Add(new Game
                {
                    Name = title,
                    InstallFolder = installPath,
                    ExecutablePath = executablePath,
                    EngineName = engine,
                    PlatformName = Game.Platform.Heroic
                });
            }
        }
        catch (Exception ex)
        {
            _logService.LogError($"Something went wrong with Epic Heroic: {ex.Message}");
        }

        return games;
    }

    #endregion

    private string? DetectExecutablePathAndEngine(string rootPath, out Game.Engine engine)
    {
        var uePath = FindUEBinariesFolder(rootPath);
        if (uePath != null)
        {
            engine = Game.Engine.Unreal;

            return uePath;
        }

        var unityPlayer = FindFileShallow(rootPath, "UnityPlayer.dll", maxDepth: 2);
        if (unityPlayer != null)
        {
            engine = Game.Engine.Unity;
            return Path.GetDirectoryName(unityPlayer);
        }

        var exeFolder = FindShallowExeFolder(rootPath);
        engine = Game.Engine.Unknown;
        return exeFolder;
    }


    private string? FindUEBinariesFolder(string? root)
    {
        if (string.IsNullOrEmpty(root)) return null;
        var candidates = new List<string>();
        CollectUEBinaries(root, 0, candidates);
        if (candidates.Count == 0) return null;

        // Prefer folders that contain a Shipping exe
        var withShipping = candidates.FirstOrDefault(c =>
            Directory.GetFiles(c, "*Shipping.exe").Length > 0 ||
            Directory.GetFiles(c, "*.exe").Any(f =>
                Path.GetFileName(f).Contains("Shipping", StringComparison.OrdinalIgnoreCase)));

        return withShipping ?? candidates[0];
    }

    private void CollectUEBinaries(string dir, int depth, List<string> results)
    {
        if (depth > 5 || string.IsNullOrEmpty(dir)) return;

        try
        {
            foreach (var sub in Directory.GetDirectories(dir))
            {
                var name = Path.GetFileName(sub);

                // Skip Engine folder — its Binaries are for the engine, not the game
                if (name.Equals("Engine", StringComparison.OrdinalIgnoreCase)) continue;

                // Found a Binaries folder — look inside for Win64/WinGDK
                if (name.Equals("Binaries", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var binSub in Directory.GetDirectories(sub))
                    {
                        var binName = Path.GetFileName(binSub);
                        if (OperatingSystem.IsWindows())
                        {
                            bool isWindowsFolder = binName.Equals("Win64", StringComparison.OrdinalIgnoreCase)
                                                   || binName.Equals("WinGDK", StringComparison.OrdinalIgnoreCase);
                            if (isWindowsFolder && Directory.GetFiles(binSub, "*.exe").Length > 0)
                                results.Add(binSub);
                        }
                        else
                        {
                            bool isLinuxFolder = binName.Equals("Linux", StringComparison.OrdinalIgnoreCase)
                                                 || binName.Equals("Linux64", StringComparison.OrdinalIgnoreCase);
                            if (isLinuxFolder && Directory.GetFiles(binSub).Length > 0)
                                results.Add(binSub);
                        }
                    }

                    // Don't recurse further into Binaries
                    continue;
                }

                // Recurse into non-Engine, non-Binaries subfolders
                CollectUEBinaries(sub, depth + 1, results);
            }
        }
        catch
        {
        }
    }

    private string? FindFileShallow(string dir, string pattern, int maxDepth)
    {
        if (maxDepth < 0 || !Directory.Exists(dir)) return null;
        try
        {
            var found = Directory.GetFiles(dir, pattern);
            if (found.Length > 0) return found[0];
            if (maxDepth > 0)
                foreach (var sub in Directory.GetDirectories(dir))
                {
                    var r = FindFileShallow(sub, pattern, maxDepth - 1);
                    if (r != null) return r;
                }
        }
        catch
        {
        }

        return null;
    }

    private string? FindShallowExeFolder(string root)
    {
        var queue = new Queue<(string path, int depth)>();
        queue.Enqueue((root, 0));
        while (queue.Count > 0)
        {
            var (dir, depth) = queue.Dequeue();
            if (depth > 4) continue;
            try
            {
                if (Directory.GetFiles(dir, "*.exe").Length > 0) return dir;
                foreach (var sub in Directory.GetDirectories(dir))
                    queue.Enqueue((sub, depth + 1));
            }
            catch
            {
            }
        }

        return null;
    }
}