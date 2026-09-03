using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.Results;

namespace Restall.Infrastructure.Scanners;

// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate

// TODO(logging-refactor): just swap the logging implementations
internal sealed partial class GOGScanner : IPlatformScannerService
{
    private readonly ILogger<GOGScanner> _logger;

    public GOGScanner(
        ILogger<GOGScanner> logger
    )
    {
        _logger = logger;
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

    private (List<Game> games, string? error) ScanGOGLibrary()
    {
        var games = new List<Game>();


        using var key = GameScanHelper.GetOpenRegistryKey(@"GOG.com\Games");
        if (key is null)  return (games, null);


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
                LogGOGLibraryScanFailed(subName, ex);
            }
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

        if (!File.Exists(installedJsonPath))
        {
            return (games, null);
        }

        string json;

        try
        {
            json = File.ReadAllText(installedJsonPath);
        }
        catch (Exception ex)
        {
            LogGOGHeroicFailedToReadJsonFile(installedJsonPath, ex);
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

                var installPath = RegexHelper.HeroicInstallPathRegex.Match(blockValue)
                    is { Success: true } pm
                    ? pm.Groups[1].Value.Replace("\\\\", "\\")
                    : null;

                installPath = GameScanHelper.NormalizePath(installPath);

                //TODO: INCLUDE THE BLOCKVALUE?
                if (string.IsNullOrEmpty(installPath))
                {
                    LogGOGHeroicInstallPathNotFound(appName);
                    continue;
                }

                var title = RegexHelper.HeroicTitleRegex.Match(blockValue)
                    is { Success: true } tm
                    ? tm.Groups[1].Value
                    : null;

                var name = !string.IsNullOrWhiteSpace(title)
                    ? title
                    : Path.GetFileName(installPath);

                if (string.IsNullOrEmpty(name))
                {
                    LogGOGHeroicGameNameNotFound(appName, installPath);
                    continue;
                }

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = installPath,
                    PlatformName = Game.Platform.GOG,
                    PlatformId = appName
                });
            }
            catch (Exception ex)
            {
                LogGOGHeroicFailedToScanJsonBlock(match.Value, ex);
            }
        }


        return (games, null);
    }
}