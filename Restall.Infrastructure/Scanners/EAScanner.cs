using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

public class EAScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    private readonly IEngineDetectionService _engineDetectionService;
    
    public Game.Platform Platform => Game.Platform.EA;

    public EAScanner(
        ILogService logService, 
        IEngineDetectionService engineDetectionService)
    {
        _logService = logService;
        _engineDetectionService = engineDetectionService;
    }
    
    public Task<List<Game>> ScanAsync() => Task.Run(ScanEA);

    private List<Game> ScanEA()
    {
        var games = new List<Game>();
        if (OperatingSystem.IsWindows())
        {
            games.AddRange(ScanEALibrary());
        }

        return games;
    }

    private List<Game> ScanEALibrary()
    {
        var games = new List<Game>();
        try
        {
            using var key = Helper.GetOpenRegistryKey(@"\EA Games");

            if (key == null) return games;
            foreach (var subName in key.GetSubKeyNames())
            {
                using var gameKey = key.OpenSubKey(subName);
                if (gameKey == null) continue;

                var installDir = gameKey.GetValue(Helper.NormalizePath("Install Dir")) as string
                                 ?? gameKey.GetValue(Helper.NormalizePath("InstallLocation")) as string
                                 ?? gameKey.GetValue(Helper.NormalizePath("InstallDir")) as string;


                if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir)) continue;

                var displayName = gameKey.GetValue("DisplayName") as string
                                  ?? subName;

                if (string.IsNullOrEmpty(displayName)) continue;

                var executablePath = _engineDetectionService.DetectExecutablePathAndEngine(installDir, out var engine);

                games.Add(new Game
                {
                    Name = displayName,
                    InstallFolder = installDir,
                    ExecutablePath = executablePath,
                    EngineName = engine,
                    PlatformName = Platform
                });
            }
        }
        catch
        {
            _logService.LogError($"Could not find EA games...");
        }

        return games;
    }

}