using System.Runtime.Versioning;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.Results;
using Restall.Application.Logging;

namespace Restall.Infrastructure.Scanners;


// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate
internal sealed partial class GOGScanner : IPlatformScannerService
{
    private readonly ILogger<GOGScanner> _logger;
    private readonly IPathService _pathService;
    
    public GOGScanner(
        ILogger<GOGScanner> logger,
        IPathService pathService)
    {
        _logger = logger;
        _pathService = pathService;
    }
    
    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanGOG);
    public Game.Platform Platform => Game.Platform.GOG;

    private GameScanResultDto ScanGOG()
    {
        var games = new List<Game>();
        var errors = new List<string>();
        if (OperatingSystem.IsWindows())
        {
            var (gogGames, error) = ScanGOGLibrary();
            games.AddRange(gogGames);
            if (error is not null) errors.Add(error);
        }
        
        var gogHeroicPath = _pathService.GetHeroicPath();
        
        if (Directory.Exists(gogHeroicPath))
        {
            var (heroicGames, error) = ScanHeroicLibrary();
            games.AddRange(heroicGames);
            if (error is not null) errors.Add(error);
        }

        return new GameScanResultDto(
            Platform: Game.Platform.GOG,
            Games: games,
            IsSuccess: games.Count > 0,
            Message: errors.Count > 0 ? string.Join(", ", errors) : null);
    }

    [SupportedOSPlatform("windows")]
    private (List<Game> games, string? error) ScanGOGLibrary()
    {
        var games = new List<Game>();


        using var key = GameScanHelper.GetOpenRegistryKey(@"GOG.com\Games");
        if (key is null)  return (games, null);
        
        foreach (var subName in key.GetSubKeyNames())
        {
            try
            {
                using var gameKey = key.OpenSubKey(subName);
                if (gameKey is null) continue;
                

                var name = GameScanHelper.GetRegistryValue(gameKey, "GAMENAME", "GameName", "gameName");
                var path = GameScanHelper.GetRegistryValue(gameKey, "PATH", "path");

                if (string.IsNullOrEmpty(name))
                {
                    LogGOGGameDisplayNameEmpty(subName);
                    continue;
                }

                if (!Directory.Exists(path))
                {
                    LogGOGInstallPathNotFound(name, subName);
                    continue;
                }

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = path,
                    PlatformName = Platform,
                    PlatformId = subName
                });
            }

            catch (Exception ex)
            {
                LogGOGLibraryScanFailure(subName, ex);
            }
        }

        return (games, null);
    }
    
    private (List<Game> games, string? error) ScanHeroicLibrary()
    {
        var games = new List<Game>();
        
        var installedJsonPath = _pathService.GetHeroicInstalledPath(Platform);
        var installedInstallInfoPath = _pathService.GetHeroicStoreCache(Platform, "gog_install_info.json");
        
        if (!File.Exists(installedJsonPath) || !File.Exists(installedInstallInfoPath))
            return (games, null);
        
        
        var installInfoGames = new Dictionary<string, string>();
        
        //TODO: Consider Regex vs JSON in both Epic and GOG Scanners
        try
        {
            var infoJson = File.ReadAllText(installedInstallInfoPath);

            foreach (Match match in RegexHelper.InstallInfoAppNameAndTitleRegex.Matches(infoJson))
            {
                var appName = match.Groups[1].Value;
                var title = match.Groups[2].Value;

                installInfoGames[appName] = title;
            }
        }
        catch (Exception ex)
        {
            _logger.HeroicInstallInfoReadFailure(Platform, installedInstallInfoPath, ex);
        }
        
        if (installInfoGames.Count == 0)
        {
            _logger.HeroicInstallInfoEmpty(Platform, installedInstallInfoPath);
            return (games, null);
        }

        string installedJson;

        try
        {
            installedJson = File.ReadAllText(installedJsonPath);
        }
        catch (Exception ex)
        {
            _logger.HeroicInstalledJsonReadFailure(Platform, installedJsonPath, ex);
            return (games, installedJsonPath);
        }
        
        foreach (Match match in RegexHelper.HeroicGameBlockRegex.Matches(installedJson))
        {
            try
            {
                var blockValue = match.Value;

                var appName = RegexHelper.GOGHeroicAppNameRegex.Match(blockValue)
                    is { Success: true } am
                    ? am.Groups[1].Value
                    : null;
                
                var installPath = RegexHelper.HeroicInstallPathRegex.Match(blockValue)
                    is { Success: true } pm
                    ? pm.Groups[1].Value.Replace("\\\\", "\\")
                    : null;
                installPath = GameScanHelper.NormalizePath(installPath);
                
                if (string.IsNullOrEmpty(appName))
                {
                    _logger.HeroicAppNameNotFound(Platform, installPath);
                    continue;
                }
                
                //TODO: INCLUDE THE BLOCKVALUE?
                if (string.IsNullOrEmpty(installPath))
                {
                    _logger.HeroicInstallPathNotFound(Platform, appName);
                    continue;
                }
                
                if (!installInfoGames.TryGetValue(appName, out var title))
                {
                    _logger.HeroicInstallInfoEntryNotFound(Platform, appName, installedInstallInfoPath);
                    continue;
                }

                if(string.IsNullOrEmpty(title))
                {
                    _logger.HeroicGameNameNotFound(Platform, appName, installPath);
                    continue;
                }
                
                games.Add(new Game
                {
                    Name = title,
                    InstallFolder = installPath,
                    PlatformName = Platform,
                    PlatformId = appName
                });
            }
            catch (Exception ex)
            {
                _logger.HeroicJsonBlockScanFailure(Platform, match.Value, ex);
            }
        }

        return (games, null);
    }
}