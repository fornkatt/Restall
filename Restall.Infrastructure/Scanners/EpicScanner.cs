using System.Text.RegularExpressions;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

public class EpicScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    private readonly IEngineDetectionService _engineDetectionService;

    public Game.Platform Platform => Game.Platform.Epic;
    
    public EpicScanner(
        ILogService logService, 
        IEngineDetectionService engineDetectionService)
    {
        _logService = logService;
        _engineDetectionService = engineDetectionService;
    }
    
    public Task<List<Game>> ScanAsync() => Task.Run(ScanEpic);

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
                var name = Helper.ExtractJsonString(json, "DisplayName");
                var rootPath = Helper.ExtractJsonString(json, "InstallLocation");

                if (rootPath != null)
                {
                    if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(rootPath))
                    {
                        _logService.LogWarning($" Skipping Epic: empty title or path");
                        continue;
                    }

                    if (!Directory.Exists(rootPath))
                    {
                        _logService.LogWarning($"Could not find install location: {rootPath}");
                        continue;
                    }

                    var executablePath = _engineDetectionService.DetectExecutablePathAndEngine(rootPath, out var engine);
                    if (executablePath != null && Helper.NonGame(executablePath))
                    {
                        _logService.LogInfo($"[EXCLUDED] {Helper.NonGame(executablePath)}");
                        continue;
                    }

                    if (string.IsNullOrEmpty(executablePath))
                    {
                        _logService.LogWarning($"Could not find executable path: {rootPath}");
                        continue;
                    }

                    games.Add(new Game
                    {
                        Name = name,
                        InstallFolder = rootPath,
                        ExecutablePath = executablePath,
                        EngineName = engine,
                        PlatformName = Platform
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
            var titleRegexMatches = Regex.Matches(json, @"""title""\s*:\s*""([^""]+)""");
            var pathRegexMatches = Regex.Matches(json, @"""install_path""\s*:\s*""([^""]+)""");


            for (int i = 0; i < Math.Min(titleRegexMatches.Count, pathRegexMatches.Count); i++)
            {
                var title = titleRegexMatches[i].Groups[1].Value;
                var installPath = pathRegexMatches[i].Groups[1].Value.Replace("\\\\", "\\");
                installPath = Helper.NormalizePath(installPath);


                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(installPath))
                {
                    _logService.LogWarning($"Could not find the install path nor title. Title: {title} InstallPath:{installPath}");
                    continue;
                }

                if (!Directory.Exists(installPath))
                {
                    _logService.LogWarning($"Install Path for Heroic {installPath} not found!");
                    continue;
                }

                var executablePath = _engineDetectionService.DetectExecutablePathAndEngine(installPath, out var engine);

                if (string.IsNullOrEmpty(executablePath)) continue;

                games.Add(new Game
                {
                    Name = title,
                    InstallFolder = installPath,
                    ExecutablePath = executablePath,
                    EngineName = engine,
                    PlatformName = Platform
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