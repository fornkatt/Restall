using System.Text.RegularExpressions;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

public class SteamScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    
    public SteamScanner(
        ILogService logService)
    {
        _logService = logService;
    }
    
    public Task<List<Game>> ScanAsync() => Task.Run(ScanSteam);
    public Game.Platform Platform => Game.Platform.Steam;

    private List<Game> ScanSteam()
    {
        var games = new List<Game>();
        var steamPath = GetInstallPath();

        if (steamPath == null) return games;
        steamPath = GameScanHelper.NormalizePath(steamPath);
        foreach (var library in GetSteamLibraries(steamPath))
        {
            games.AddRange(ScanSteamLibrary(library));
        }

        return games;
    }

    private string? GetInstallPath()
    {
        if (OperatingSystem.IsWindows()) return GameScanHelper.ReadRegistry(@"Valve\Steam", "SteamPath");
        
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
                var name = GameScanHelper.ExtractVdfValue(content, "name");
                var installDir = GameScanHelper.ExtractVdfValue(content, "installdir");
                
                if (name == null || installDir == null) continue;

                if (GameScanHelper.NonGame(name))
                {
                    _logService.LogInfo($"[EXCLUDED] Non-game name: {name}");
                    continue;
                }
                if (GameScanHelper.NonGame(installDir))
                {
                    _logService.LogInfo($"[EXCLUDED] Non-game by path: {installDir}");
                    continue;
                }

                var rootPath = Path.Combine(steamapps, "common", installDir);
                if (!Directory.Exists(rootPath)) continue;
                var appId = Path.GetFileNameWithoutExtension(acf).Replace("appmanifest_", "");
                
                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = rootPath,
                    PlatformName = Platform,
                    PlatformId = $"steam:{appId}"
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
        foreach (Match match in RegexHelper.SteamLibraryRegex.Matches(File.ReadAllText(vdfPath)))
        {
            var library = GameScanHelper.NormalizePath(match.Groups[1].Value.Replace(@"\\", @"\"));
            if (Directory.Exists(library) && !libraries.Contains(library))
            {
                libraries.Add(library);
            }
        }

        return libraries;
    }
    
}