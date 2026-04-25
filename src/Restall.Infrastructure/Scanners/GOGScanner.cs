using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Text.RegularExpressions;
using Restall.Application.DTOs.Results;

namespace Restall.Infrastructure.Scanners;
/// <summary>
/// I selected GOG Scanner as main reference for giving an understanding how all the scanners work
/// 
/// Each scanner are following the same pattern, scan the platform, registry or file,
/// collect the games into a tuple and pass on the results.
/// All launchers are going through the process of scanning two sources, registry and json
/// </summary>
internal sealed class GOGScanner : IPlatformScannerService
{
    private readonly ILogService _logService;

    public GOGScanner(ILogService logService)
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
            var (gogGames, error) = ScanGOGLibrary();
            games.AddRange(gogGames);
            if (error is not null) errors.Add(error);
        }

        var gogHeroicPath = GetHeroicInstallPath();
        if (gogHeroicPath is not null && Directory.Exists(gogHeroicPath))
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

    private (List<Game> games, string? error) ScanGOGLibrary()
    {
        var games = new List<Game>();

        //Local Machine
        using var key = GameScanHelper.GetOpenRegistryKey(@"GOG.com\Games");

        if (key is null) return (games, null);

#pragma warning disable CA1416 // Already checked before method is called
        foreach (var subName in key.GetSubKeyNames())
        {
            try
            {
                using var gameKey = key.OpenSubKey(subName);
                if (gameKey is null) continue;

                //Value patterns in registry
                var name = GameScanHelper.GetRegistryValue(gameKey, "GAMENAME", "GameName", "gameName");
                var path = GameScanHelper.GetRegistryValue(gameKey, "PATH", "path");

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path)) continue;
                if (!Directory.Exists(path)) continue;

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = path,
                    PlatformName = Platform,
                    PlatformId = $"gog:{subName}" //exact name usage for API search in SteamGridDb
                });
            }

            catch (Exception ex)
            {
                _logService.LogError("Failed to process GOG games", ex);
            }
        }

        return (games, null);
    }

    private string? GetHeroicInstallPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        //Windows and Linux path
        var heroicPath = OperatingSystem.IsWindows()
            ? Path.Combine(home, "AppData", "Roaming", "heroic", "gog_store")
            : Path.Combine(home, ".config", "heroic", "gog_store");

        return Directory.Exists(heroicPath) ? heroicPath : null;
    }

    private (List<Game> games, string? error) ScanHeroicLibrary(string configDir)
    {
        var games = new List<Game>();
        var installedJsonPath = Path.Combine(configDir, "installed.json");

        if (!File.Exists(installedJsonPath)) return (games, null);

        string json;

        try
        {
            json = File.ReadAllText(installedJsonPath);
        }
        catch (Exception ex)
        {
            //error handling handled both through the UI for the user and through the logs for developers
            _logService.LogError($"Failed to read installed.json file in GOG Heroic library", ex);
            return (games, $"Failed to read installed.json file in GOG Heroic library.");
        }


        foreach (Match match in RegexHelper.HeroicGameBlockRegex.Matches(json)) //all data inside the installed.json file
        {
            try
            {
                var blockValue = match.Value;

                var installPath = RegexHelper.HeroicInstallPathRegex.Match(blockValue)
                    is { Success: true } pm
                    ? pm.Groups[1].Value.Replace("\\\\", "\\")
                    : null;

                installPath = GameScanHelper.NormalizePath(installPath);
                if (string.IsNullOrEmpty(installPath) || !Directory.Exists(installPath)) continue;

                var title = RegexHelper.HeroicTitleRegex.Match(blockValue)
                    is { Success: true } tm
                    ? tm.Groups[1].Value
                    : null;

                var name = !string.IsNullOrWhiteSpace(title)
                    ? title
                    : Path.GetFileName(installPath);

                if (string.IsNullOrEmpty(name)) continue;

                var appName = RegexHelper.HeroicAppNameRegex.Match(blockValue)
                    is { Success: true } am
                    ? am.Groups[1].Value
                    : null;

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = installPath,
                    PlatformName = Game.Platform.GOG,
                    PlatformId = $"gog:{appName}"
                });
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to scan the json block in GOG Heroic library", ex);
            }
        }


        return (games, null);
    }
}