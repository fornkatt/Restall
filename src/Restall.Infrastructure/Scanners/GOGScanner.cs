// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Logging;
using Restall.Domain.Entities;
using Restall.Infrastructure.DTOs.Heroic;
using Restall.Infrastructure.Helpers;
using Restall.Infrastructure.Scanners.Heroic;
using System.Runtime.Versioning;



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
        if (key is null) return (games, null);

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
        var gogLibraryJsonPath = _pathService.GetHeroicStoreCache(Platform, "gog_library.json");

        if (!File.Exists(installedJsonPath) || !File.Exists(gogLibraryJsonPath))
            return (games, null);


        List<HeroicInstalledGame> installedGames;
        try
        {
            var installedJson = File.ReadAllText(installedJsonPath);
            installedGames = HeroicInstalledParser.InstalledParser(installedJson, Platform);
        }
        catch (Exception ex)
        {
            _logger.HeroicInstalledJsonReadFailure(Platform, installedJsonPath, ex);
            return (games, installedJsonPath);
        }

        if (installedGames.Count == 0)
        {
            _logger.HeroicInstalledEmpty(Platform, installedJsonPath);
            return (games, null);
        }

        var libraryTitles = new Dictionary<string, string>();

        try
        {
            var libraryJson = File.ReadAllText(gogLibraryJsonPath);
            var heroicLibrary = HeroicLibraryParser.LibraryParser(libraryJson, Platform);

            foreach (var entry in heroicLibrary)
            {
                libraryTitles.TryAdd(entry.AppName, entry.Title);
            }
        }
        catch (Exception ex)
        {
            _logger.HeroicLibraryReadFailure(Platform, gogLibraryJsonPath, ex);
            return (games, gogLibraryJsonPath);
        }

        if (libraryTitles.Count == 0)
        {
            _logger.HeroicLibraryEmpty(Platform, gogLibraryJsonPath);
            return (games, null);
        }

        foreach (var entry in installedGames)
        {
            try
            {
                if (entry.IsDlc) continue;

                if (string.IsNullOrEmpty(entry.AppName))
                {
                    _logger.HeroicAppNameNotFound(Platform, entry.InstallPath);
                    continue;
                }

                var installPath = GameScanHelper.NormalizePath(entry.InstallPath);

                if (string.IsNullOrEmpty(installPath))
                {
                    _logger.HeroicInstallPathNotFound(Platform, entry.AppName);
                    continue;
                }

                if (!libraryTitles.TryGetValue(entry.AppName, out var title))
                {
                    _logger.HeroicLibraryEntryNotFound(Platform, entry.AppName, gogLibraryJsonPath);
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
                    InstallFolder = installPath,
                    PlatformName = Platform,
                    PlatformId = entry.AppName
                });
            }
            catch (Exception ex)
            {
                _logger.HeroicEntryScanFailure(Platform, entry.ToString(), ex);
            }
        }

        return (games, null);
    }
}
