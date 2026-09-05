using System.Runtime.Versioning;
using System.Text.Json;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Text.RegularExpressions;
using Restall.Application.DTOs.Results;

namespace Restall.Infrastructure.Scanners;

internal sealed class GOGScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    private readonly IPathService _pathService;

    public GOGScanner(ILogService logService,
        IPathService pathService)
    {
        _logService = logService;
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
            var (heroicGames, error) = ScanHeroicLibrary(gogHeroicPath);
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

        //Local Machine
        using var key = GameScanHelper.GetOpenRegistryKey(@"GOG.com\Games");

        if (key is null) return (games, null);


        foreach (var subName in key.GetSubKeyNames())
        {
            try
            {
                using var gameKey = key.OpenSubKey(subName);
                if (gameKey is null) continue;

                var name = GameScanHelper.GetRegistryValue(gameKey, "GAMENAME", "GameName", "gameName");
                var path = GameScanHelper.GetRegistryValue(gameKey, "PATH", "path");

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path)) continue;
                if (!Directory.Exists(path)) continue;

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
                _logService.LogError("Failed to process GOG games", ex);
            }
        }

        return (games, null);
    }


    private (List<Game> games, string? error) ScanHeroicLibrary(string configDir)
    {
        var games = new List<Game>();
        var installedJsonPath = Path.Combine(configDir, "gog_store", "installed.json");
        var installedInstallInfoPath = Path.Combine(configDir, "store_cache", "gog_install_info.json");

        if (!File.Exists(installedJsonPath) || !File.Exists(installedInstallInfoPath)) return (games, null);
        
        var installInfoGames = new Dictionary<string, string>();
        string json;

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
            _logService.LogError($"Failed to read gog_install_info.json file in GOG Heroic library", ex);
        }

        try
        {
            json = File.ReadAllText(installedJsonPath);
        }
        catch (Exception ex)
        {
            _logService.LogError($"Failed to read installed.json file in GOG Heroic library", ex);
            return (games, $"Failed to read installed.json file in GOG Heroic library.");
        }

        foreach (Match match in RegexHelper.HeroicGameBlockRegex.Matches(json))
        {
            try
            {
                var blockValue = match.Value;

                var appName = RegexHelper.GOGHeroicAppNameRegex.Match(blockValue)
                    is { Success: true } am
                    ? am.Groups[1].Value
                    : null;

                if (string.IsNullOrEmpty(appName)) continue;

                if (!installInfoGames.TryGetValue(appName, out var title)) continue;

                var installPath = RegexHelper.HeroicInstallPathRegex.Match(blockValue)
                    is { Success: true } pm
                    ? pm.Groups[1].Value.Replace("\\\\", "\\")
                    : null;


                installPath = GameScanHelper.NormalizePath(installPath);
                if (string.IsNullOrEmpty(installPath)) continue;

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
                _logService.LogError($"Failed to scan the json block in GOG library", ex);
            }
        }

        return (games, null);
    }
}