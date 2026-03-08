using System.Text.RegularExpressions;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

public class SteamScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    private readonly IEngineDetectionService _engineDetectionService;
    
    public Game.Platform Platform => Game.Platform.Steam;

    public SteamScanner(
        ILogService logService, 
        IEngineDetectionService engineDetectionService)
    {
        _logService = logService;
        _engineDetectionService = engineDetectionService;
    }
    
    public Task<List<Game>> ScanAsync() => Task.Run(ScanSteam);

    private List<Game> ScanSteam()
    {
        var games = new List<Game>();
        var steamPath = GetInstallPath();

        if (steamPath == null) return games;
        steamPath = Helper.NormalizePath(steamPath);
        foreach (var library in GetSteamLibraries(steamPath))
        {
            games.AddRange(ScanSteamLibrary(library));
        }

        return games;
    }

    private string? GetInstallPath()
    {
        if (OperatingSystem.IsWindows()) return Helper.ReadRegistry(@"Valve\Steam", "SteamPath");
        
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

                var executablePath = _engineDetectionService.DetectExecutablePathAndEngine(rootPath, out var engine);

                if (string.IsNullOrEmpty(executablePath)) continue;

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = rootPath,
                    ExecutablePath = executablePath,
                    EngineName = engine,
                    PlatformName = Platform
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
    
}