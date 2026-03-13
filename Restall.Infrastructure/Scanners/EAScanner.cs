using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

public class EAScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    
    public EAScanner(
        ILogService logService)
    {
        _logService = logService;
    }
    
    public Task<List<Game>> ScanAsync() => Task.Run(ScanEA);
    public Game.Platform Platform => Game.Platform.EA;


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
            using var key = GameScanHelper.GetOpenRegistryKey(@"\EA Games");

            if (key == null) return games;
            foreach (var subName in key.GetSubKeyNames())
            {
                using var gameKey = key.OpenSubKey(subName);
                if (gameKey == null) continue;

                var installDir = gameKey.GetValue(GameScanHelper.NormalizePath("Install Dir")) as string
                                 ?? gameKey.GetValue(GameScanHelper.NormalizePath("InstallLocation")) as string
                                 ?? gameKey.GetValue(GameScanHelper.NormalizePath("InstallDir")) as string;


                if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir)) continue;

                var displayName = gameKey.GetValue("DisplayName") as string
                                  ?? subName;

                if (string.IsNullOrEmpty(displayName)) continue;

                

                games.Add(new Game
                {
                    Name = displayName,
                    InstallFolder = installDir,
                    PlatformName = Platform,
                    PlatformId = $"origin:{subName}"
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