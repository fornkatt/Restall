// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Logging;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using Restall.Infrastructure.Scanners.Heroic;
using System.Text.Json;


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
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var name = GameScanHelper.ReadJsonString(root, "DisplayName");
                var rootPath = GameScanHelper.ReadJsonString(root, "InstallLocation");
                var appName = GameScanHelper.ReadJsonString(root, "AppName");


                if (string.IsNullOrEmpty(name))
                {
                    LogEpicGameNameNotFound(file, item);
                    continue;
                }

                var installPath = GameScanHelper.NormalizePath(rootPath);

                if (string.IsNullOrEmpty(installPath))
                {
                    LogEpicGameInstallPathNotFound(name, item);
                    continue;
                }

                if (!Directory.Exists(installPath))
                {
                    LogEpicGameInstallFolderNotFound(installPath,name, item);
                    continue;
                }

                if (GameScanHelper.NonGame(installPath)) continue;

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = installPath,
                    PlatformName = Platform,
                    PlatformId = appName
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
        var epicLibraryJsonPath = _pathService.GetHeroicStoreCache(Platform, "legendary_library.json");

         if (!File.Exists(installedJsonPath) || !File.Exists(epicLibraryJsonPath))
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
            var libraryJson = File.ReadAllText(epicLibraryJsonPath);
            var heroicLibrary = HeroicLibraryParser.LibraryParser(libraryJson, Platform);

            foreach (var entry in heroicLibrary)
            {
                libraryTitles.TryAdd(entry.AppName, entry.Title);
            }
        }
        catch (Exception ex)
        {
            _logger.HeroicLibraryReadFailure(Platform,epicLibraryJsonPath,ex);
            return (games, epicLibraryJsonPath);
        }

        if (libraryTitles.Count == 0)
        {
            _logger.HeroicLibraryEmpty(Platform, epicLibraryJsonPath);
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

                if(!libraryTitles.TryGetValue(entry.AppName, out var title))
                {
                    _logger.HeroicLibraryEntryNotFound(Platform, entry.AppName, epicLibraryJsonPath);
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



