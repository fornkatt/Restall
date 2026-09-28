// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Logging;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using Restall.Infrastructure.Scanners.Heroic;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Scanners;

// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate
internal sealed partial class EpicScanner : IPlatformScannerService
{
    private readonly ILogger<EpicScanner> _logger;
    private readonly IPathService _pathService;

    public EpicScanner(
        ILogger<EpicScanner> logger,
        IPathService pathService)
    {
        _logger = logger;
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
            var item = Path.GetFileNameWithoutExtension(file);
            try
            {
                var json = File.ReadAllText(file);
                var name = GameScanHelper.ExtractJsonString(json, "DisplayName");
                var rootPath = GameScanHelper.ExtractJsonString(json, "InstallLocation");
                var catalogItemId = GameScanHelper.ExtractJsonString(json, "CatalogItemId");


                if (string.IsNullOrEmpty(name))
                {
                    LogEpicGameNameNotFound(file, item);
                    continue;
                }

                if (!Directory.Exists(rootPath))
                {
                    LogEpicGameRootPathNotFound(name, item);
                    continue;
                }

                if (GameScanHelper.NonGame(rootPath)) continue;

                games.Add(new Game
                {
                    Name = name, InstallFolder = rootPath, PlatformName = Platform, PlatformId = catalogItemId
                });
            }
            catch (Exception ex)
            {
                LogEpicManifestScanFailure(file, ex);
            }
        }

        return (games, null);
    }

    private (List<Game> games, string? error) ScanHeroicLibrary()
    {
        var games = new List<Game>();
        var installedJsonPath = _pathService.GetHeroicInstalledPath(Platform);
        var installInfoPath = _pathService.GetHeroicStoreCache(Platform, "legendary_install_info.json");

        if (!File.Exists(installedJsonPath) || !File.Exists(installInfoPath))
            return (games, null);

        var installInfoGames = new Dictionary<string, string>();

        try
        {
            var infoJson = File.ReadAllText(installInfoPath);

            foreach (Match match in RegexHelper.InstallInfoAppNameAndTitleRegex.Matches(infoJson))
            {
                var appName = match.Groups[1].Value;
                var title = match.Groups[2].Value;

                installInfoGames[appName] = title;
            }
        }
        catch (Exception ex)
        {
            _logger.HeroicInstallInfoReadFailure(Platform, installInfoPath, ex);
        }

        if (installInfoGames.Count == 0)
        {
            _logger.HeroicInstallInfoEmpty(Platform, installInfoPath);
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

        foreach (var entry in EpicHeroicParser(installedJson))
        {
            try
            {
                if (string.IsNullOrEmpty(entry.AppName))
                {
                    _logger.HeroicAppNameNotFound(Platform, entry.InstallPath);
                    continue;
                }

                if (!installInfoGames.TryGetValue(entry.AppName, out var title))
                {
                    _logger.HeroicInstallInfoEntryNotFound(Platform, entry.AppName, installInfoPath);
                    continue;
                }

                if (string.IsNullOrEmpty(entry.InstallPath))
                {
                    _logger.HeroicInstallPathNotFound(Platform, entry.AppName);
                    continue;
                }

                if (string.IsNullOrEmpty(title))
                {
                    _logger.HeroicGameNameNotFound(Platform, entry.AppName, entry.InstallPath);
                    continue;
                }

                games.Add(new Game
                {
                    Name = title,
                    InstallFolder = entry.InstallPath,
                    PlatformName = Platform,
                    PlatformId = entry.AppName
                });
            }
            catch (Exception ex)
            {
                _logger.HeroicJsonBlockScanFailure(Platform, installedJson, ex);
            }
        }

        return (games, null);
    }


    internal static List<HeroicInstalledGame> EpicHeroicParser(string json)
    {
        var games = new List<HeroicInstalledGame>();

        foreach (Match match in RegexHelper.HeroicGameBlockRegex.Matches(json))
        {
            var blockValue = match.Value;

            var appName = RegexHelper.EpicHeroicAppNameRegex.Match(blockValue)
                is { Success: true } am
                ? am.Groups[1].Value
                : null;

            var installPath = RegexHelper.HeroicInstallPathRegex.Match(blockValue)
                is { Success: true } pm
                ? pm.Groups[1].Value.Replace("\\\\", "\\")
                : null;

            installPath = GameScanHelper.NormalizePath(installPath);

            var isDlcMatch = Regex.IsMatch(blockValue, @"""is_dlc""\s*:\s*true", RegexOptions.IgnoreCase);

            if (isDlcMatch) continue;

            games.Add(new HeroicInstalledGame(appName, installPath, isDlcMatch));
        }

        return games;
    }
}



