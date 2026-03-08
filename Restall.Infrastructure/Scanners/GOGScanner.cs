using System.Text.RegularExpressions;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

public class GOGScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    private readonly IEngineDetectionService _engineDetectionService;

    public GOGScanner(
        ILogService logService, 
        IEngineDetectionService engineDetectionService)
    {
        _logService = logService;
        _engineDetectionService = engineDetectionService;
    }
    
    public Game.Platform Platform => Game.Platform.GOG;
    
    public Task<List<Game>> ScanAsync() => Task.Run(ScanGOG);

    private List<Game> ScanGOG()
    {
        var games = new List<Game>();

        if (OperatingSystem.IsWindows())
        {
            games.AddRange(ScanGOGLibrary());
        }

        var gogHeroicPath = GetHeroicInstallPath();
        if (gogHeroicPath != null && Directory.Exists(gogHeroicPath))
        {
            games.AddRange(ScanHeroicLibrary(gogHeroicPath));
        }

        return games;
    }

    private List<Game> ScanGOGLibrary()
    {
        var games = new List<Game>();


        try
        {
            using var key = Helper.GetOpenRegistryKey(@"GOG.com\Games");

            if (key == null) return games;


            foreach (var sub in key.GetSubKeyNames())
            {
                using var gameKey = key.OpenSubKey(sub);

                if (gameKey == null) continue;

                var name = gameKey.GetValue("GAMENAME") as string
                           ?? gameKey.GetValue("GameName") as string;

                var path = gameKey.GetValue("PATH") as string
                           ?? gameKey.GetValue("path") as string;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path)) continue;
                if (!Directory.Exists(path)) continue;

                var executablePath = _engineDetectionService.DetectExecutablePathAndEngine(path, out var engine);
                if (string.IsNullOrEmpty(executablePath)) continue;

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = path,
                    ExecutablePath = executablePath,
                    EngineName = engine,
                    PlatformName = Platform
                });
            }
        }
        catch
        {
            _logService.LogError($"Could not find GOG games...{games}");
        }

        return games;
    }
    
    private string? GetHeroicInstallPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var heroicPath = OperatingSystem.IsWindows()
            ? Path.Combine(home, "AppData", "Roaming", "heroic", "gog_store")
            : Path.Combine(home, ".config", "heroic", "gog_store");

        return Directory.Exists(heroicPath) ? heroicPath : null;
    }

    private List<Game> ScanHeroicLibrary(string configDir)
    {
        var games = new List<Game>();
        var installedJsonPath = Path.Combine(configDir, "installed.json");

        if (!File.Exists(installedJsonPath)) return games;

        try
        {
            var json = File.ReadAllText(installedJsonPath);
            var pathRegexMatches = Regex.Matches(json, @"""install_path""\s*:\s*""([^""]+)""");

            foreach (Match match in pathRegexMatches)
            {
                var installPath = match.Groups[1].Value.Replace("\\\\", "\\");
                installPath = Helper.NormalizePath(installPath);

                if (string.IsNullOrEmpty(installPath)) continue;
                if (!Directory.Exists(installPath)) continue;

                var title = Path.GetFileName(installPath);
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
            _logService.LogError($"Could not find Gog heroic library: {installedJsonPath} {ex.Message}");
        }

        return games;
    }
    
    

}