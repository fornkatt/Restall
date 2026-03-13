using System.Text.RegularExpressions;
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
    
    public Task<List<Game>> ScanAsync() => Task.Run(ScanEpic);
    public Game.Platform Platform => Game.Platform.Epic;

    private List<Game> ScanEpic()
    {
        var games = new List<Game>();

        if (OperatingSystem.IsWindows())
        {
            var ueInstallPath = GetInstallPath();


            if (ueInstallPath != null && Directory.Exists(ueInstallPath))
            {
                games.AddRange(ScanEpicLibrary(ueInstallPath));
            }
        }

        var epicHeroicPath = GetHeroicInstallPath();
        if (epicHeroicPath != null && Directory.Exists(epicHeroicPath))
        {
            games.AddRange(ScanHeroicLibrary(epicHeroicPath));
        }

        return games;
    }

    private string? GetInstallPath()
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
            catch
            {
                _logService.LogError("Could not find Epic library: " + file);
            }
        }

        return games;
    }
    private string? GetHeroicInstallPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var heroicPath = OperatingSystem.IsWindows()
            ? Path.Combine(home, "AppData", "Roaming", "heroic", "legendaryConfig", "legendary")
            : Path.Combine(home, ".config", "heroic", "legendaryConfig", "legendary");

        return Directory.Exists(heroicPath) ? heroicPath : null;
    }

    private List<Game> ScanHeroicLibrary(string configDir)
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
                    PlatformName = Platform,
                    PlatformId = $"epic:{appName}"
                    
                });
                
            }
        }
        catch (Exception ex)
        {
            _logService.LogError($"Something went wrong with Epic Heroic: {ex.Message}");
        }

        return games;
    }

}