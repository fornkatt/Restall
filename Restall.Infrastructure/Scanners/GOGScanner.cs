using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Scanners;

internal sealed class GOGScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    
    public GOGScanner(
        ILogService logService)
    {
        _logService = logService;
    }
    
    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanGOG);
    public Game.Platform Platform => Game.Platform.GOG;
    
    private GameScanResultDto ScanGOG()
    {
        var games = new List<Game>();
        var errors = new List<string>();
        if (OperatingSystem.IsWindows())
        {
            var (gogGames, error)  = ScanGOGLibrary();
            games.AddRange(gogGames);
            if(error is not null) errors.Add(error);
        }
        
        var gogHeroicPath = GetHeroicInstallPath();
        if (gogHeroicPath is not null && Directory.Exists(gogHeroicPath))
        {
            var (heroicGames, error)  = ScanHeroicLibrary(gogHeroicPath);
            games.AddRange(heroicGames);
            if(error is not null) errors.Add(error);
        }
        
        return new GameScanResultDto(
            Platform:     Game.Platform.GOG,
            Games:        games,
            Success:      games.Count > 0,
            Message: errors.Count > 0 ? string.Join("; ", errors) : null);
        
    }

    private (List<Game> games, string? error) ScanGOGLibrary()
    {
        var games = new List<Game>();


        try
        {
            using var key = GameScanHelper.GetOpenRegistryKey(@"GOG.com\Games");

            if (key is null) return (games, null);


            foreach (var subName in key.GetSubKeyNames())
            {
                using var gameKey = key.OpenSubKey(subName);

                if (gameKey is null) continue;

                var name = GameScanHelper.GetRegistryValue(gameKey, "GAMENAME", "GameName");
                var path = GameScanHelper.GetRegistryValue(gameKey, "PATH", "path");
                
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path)) continue;
                if (!Directory.Exists(path)) continue;
                
                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = path,
                    PlatformName = Platform,
                    PlatformId = $"gog:{subName}"
                });
            }
        }
        catch
        {
            _logService.LogError($"Could not find GOG games...{games}");
        }

        return (games, null);
    }
    
    private string? GetHeroicInstallPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var heroicPath = OperatingSystem.IsWindows()
            ? Path.Combine(home, "AppData", "Roaming", "heroic", "gog_store")
            : Path.Combine(home, ".config", "heroic", "gog_store");

        return Directory.Exists(heroicPath) ? heroicPath : null;
    }

    private (List<Game> games, string? error) ScanHeroicLibrary(string configDir)
    {
        var games = new List<Game>();
        var installedJsonPath = Path.Combine(configDir, "installed.json");

        if (!File.Exists(installedJsonPath)) return (games,null);

        try
        {
            var json = File.ReadAllText(installedJsonPath);
            foreach (Match match in RegexHelper.HeroicGameBlockRegex.Matches(json))
            {
                var blockValue = match.Value;

                var installPath = RegexHelper.HeroicInstallPathRegex.Match(blockValue)
                    is { Success: true } pm ? pm.Groups[1].Value.Replace("\\\\", "\\") : null;
                
                installPath = GameScanHelper.NormalizePath(installPath);
                if (string.IsNullOrEmpty(installPath) || !Directory.Exists(installPath)) continue;
                
                var title = RegexHelper.HeroicTitleRegex.Match(blockValue)
                    is {Success: true } tm ? tm.Groups[1].Value : null;
                
                var name = !string.IsNullOrWhiteSpace(title) 
                    ? title : Path.GetFileName(installPath);
                
                if (string.IsNullOrEmpty(name)) continue;
                
                var appName = RegexHelper.HeroicAppNameRegex.Match(blockValue)
                    is { Success: true } am ? am.Groups[1].Value : null;
                
                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = installPath,
                    PlatformName = Game.Platform.GOG,
                    PlatformId = $"gog:{appName}"
                });

            }

        }
        catch (Exception ex)
        {
            _logService.LogError($"Could not find Gog heroic library: {installedJsonPath} {ex.Message}");
        }

        return (games,null);
    }
    
}