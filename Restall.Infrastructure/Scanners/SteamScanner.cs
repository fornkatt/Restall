using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Scanners;

internal sealed class SteamScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    
    public SteamScanner(
        ILogService logService)
    {
        _logService = logService;
    }
    
    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanSteam);
    public Game.Platform Platform => Game.Platform.Steam;
    private GameScanResultDto ScanSteam()
    {
        var games = new List<Game>();
        var errors = new List<string>();
        var steamPath = GetInstallPath();

        if (steamPath is null)
            return new GameScanResultDto( 
                Game.Platform.Steam,
                [], 
                Success: false, 
                Message: "Steam installation not found");
        
        steamPath = GameScanHelper.NormalizePath(steamPath);
        foreach (var library in GetSteamLibraries(steamPath))
        {
            var (libraryGames, error)  = ScanSteamLibrary(library);
            games.AddRange(libraryGames);
            if (error is not null) errors.Add(error);
        }

        return new GameScanResultDto(
            Platform:     Game.Platform.Steam,
            Games:        games,
            Success:      games.Count > 0,
            Message: errors.Count > 0 ? string.Join("; ", errors) : null);
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

    private (List<Game> games, string? error) ScanSteamLibrary(string library)
    {
        var games = new List<Game>();
        var steamapps = Path.Combine(library, "steamapps");
        if (!Directory.Exists(steamapps)) return (games, null);
        foreach (var acf in Directory.GetFiles(steamapps, "appmanifest_*.acf"))
        {
            try
            {
                var content = File.ReadAllText(acf);
                var name = GameScanHelper.ExtractVdfValue(content, "name");
                var installDir = GameScanHelper.ExtractVdfValue(content, "installdir");
                
                if (name == null || installDir == null) continue;

                if (GameScanHelper.NonGame(name)) continue;
                
                if (GameScanHelper.NonGame(installDir)) continue;
                

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

        return (games, null);
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