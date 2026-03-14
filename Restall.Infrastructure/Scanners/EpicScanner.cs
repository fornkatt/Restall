using System.Text.RegularExpressions;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

public class EpicScanner : IPlatformScannerService
{
    private readonly ILogService _logService;

    public EpicScanner(
        ILogService logService)
    {
        _logService = logService;
        
    }
    
    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanEpic);
    public Game.Platform Platform => Game.Platform.Epic;
    
    private GameScanResultDto ScanEpic()
    {
        var games = new List<Game>();
        var errors = new List<string>();

        if (OperatingSystem.IsWindows())
        {
            var ueInstallPath = GetInstallPath();
            if (ueInstallPath is not null && Directory.Exists(ueInstallPath))
            {
                var (epicLibrary, error) = ScanEpicLibrary(ueInstallPath);
                games.AddRange(epicLibrary);
                if(error is not null) errors.Add(error);
            }
        }

        var epicHeroicPath = GetHeroicInstallPath();
        if (epicHeroicPath is not null && Directory.Exists(epicHeroicPath))
        {
            var (epicHeroicLibrary, error) = ScanHeroicLibrary(epicHeroicPath);
            games.AddRange(epicHeroicLibrary);
            if(error is not null) errors.Add(error);
        }
        
        return new GameScanResultDto(
            Platform:     Game.Platform.Epic,
            Games:        games,
            Success:      games.Count > 0,
            ErrorMessage: errors.Count > 0 ? string.Join("; ", errors) : null);

        
    }

    private string? GetInstallPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic", "EpicGamesLauncher", "Data", "Manifests");
    }

    private (List<Game> games, string? error) ScanEpicLibrary(string manifestDir)
    {
        var games = new List<Game>();

        foreach (var file in Directory.GetFiles(manifestDir, "*.item"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var name = GameScanHelper.ExtractJsonString(json, "DisplayName");
                var rootPath = GameScanHelper.ExtractJsonString(json, "InstallLocation");
                var catalogItemId = GameScanHelper.ExtractJsonString(json, "CatalogItemId");

                if (rootPath != null)
                {
                    if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(rootPath))
                        continue;


                    if (!Directory.Exists(rootPath))
                        continue;



                    if (GameScanHelper.NonGame(rootPath))

                        continue;


                    games.Add(new Game
                    {
                        Name = name,
                        InstallFolder = rootPath,
                        PlatformName = Platform,
                        PlatformId = $"epic:{catalogItemId}"
                    });
                }

            }
            catch (Exception ex)
            {
                    _logService.LogError($"Failed to scan Epic Library...{ex.Message}");           
            }
            
        }
        return (games, null);

    }
    private string? GetHeroicInstallPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var heroicPath = OperatingSystem.IsWindows()
            ? Path.Combine(home, "AppData", "Roaming", "heroic", "legendaryConfig", "legendary")
            : Path.Combine(home, ".config", "heroic", "legendaryConfig", "legendary");

        return Directory.Exists(heroicPath) ? heroicPath : null;
    }

    private (List<Game>games, string? error) ScanHeroicLibrary(string configDir)
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
                    is {Success: true} pm ? pm.Groups[1].Value.Replace("\\\\","\\") : null;
                installPath = GameScanHelper.NormalizePath(installPath);
                if (string.IsNullOrEmpty(installPath)) continue;
                
                var title = RegexHelper.HeroicTitleRegex.Match(blockValue)
                    is {Success: true} tm ? tm.Groups[1].Value : null;
                
                var name = !string.IsNullOrWhiteSpace(title)
                    ? title
                    : Path.GetFileName(installPath);
                
                if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(installPath)) continue;
                
                var appName = RegexHelper.HeroicAppNameRegex.Match(blockValue)
                    is { Success: true } am ? am.Groups[1].Value : null;
                
                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = installPath,
                    PlatformName = Game.Platform.Epic,
                    PlatformId = $"epic:{appName}"
                    
                });
                
            }
            
            return (games,null);
            
        }
        catch (Exception ex)
        {
            return(games,$"Something went wrong with Epic Heroic: {ex.Message}");
        }

    }
}