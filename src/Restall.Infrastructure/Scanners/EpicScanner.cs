using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Text.RegularExpressions;
using Restall.Application.DTOs.Results;

namespace Restall.Infrastructure.Scanners;

internal sealed class EpicScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    private readonly IPathService _pathService;

    public EpicScanner(ILogService logService,
        IPathService pathService)
    {
        _logService = logService;
        _pathService = pathService;
    }

    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanEpic);
    public Game.Platform Platform => Game.Platform.Epic;

    private GameScanResultDto ScanEpic()
    {
        var games = new List<Game>();
        var errors = new List<string>();

        if (OperatingSystem.IsWindows())
        {
            var ueInstallPath = _pathService.GetEpicInstallPath();
            if (Directory.Exists(ueInstallPath))
            {
                var (epicLibrary, error) = ScanEpicLibrary(ueInstallPath);
                games.AddRange(epicLibrary);
                if (error is not null) errors.Add(error);
            }
        }

        var epicHeroicPath = _pathService.GetHeroicPath();

        if (Directory.Exists(epicHeroicPath))
        {
            var (epicHeroicLibrary, error) = ScanHeroicLibrary();
            games.AddRange(epicHeroicLibrary);
            if (error is not null) errors.Add(error);
        }

        return new GameScanResultDto(
            Platform: Game.Platform.Epic,
            Games: games,
            IsSuccess: games.Count > 0,
            Message: errors.Count > 0 ? string.Join(", ", errors) : null);
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

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(rootPath))
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
                    PlatformId = catalogItemId
                });
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to scan Epic Manifest", ex);
            }
        }

        return (games, null);
    }


    private (List<Game>games, string? error) ScanHeroicLibrary()
    {
        var games = new List<Game>();
        var installedJsonPath = _pathService.GetHeroicInstalledPath(Platform);
        var installedInstallInfoPath = _pathService.GetHeroicStoreCache(Platform, "legendary_install_info.json");

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
            _logService.LogError($"Failed to read legendary_install_info.json file in Epic Heroic library", ex);
        }

        try
        {
            json = File.ReadAllText(installedJsonPath);
        }
        catch (Exception ex)
        {
            _logService.LogError($"Failed to read installed.json file in Epic Heroic library", ex);
            return (games, $"Failed to read installed.json file in Epic Heroic library.");
        }

        foreach (Match match in RegexHelper.HeroicGameBlockRegex.Matches(json))
        {
            try
            {
                var blockValue = match.Value;

                var isDlcMatch = Regex.IsMatch(blockValue, @"""is_dlc""\s*:\s*true", RegexOptions.IgnoreCase);
                
                if (isDlcMatch) continue;
                
                var appName = RegexHelper.EpicHeroicAppNameRegex.Match(blockValue)
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
                _logService.LogError($"Failed to scan the json block in Epic Heroic library", ex);
            }
        }

        return (games, null);
    }
}