using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

public class UbisoftScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    
    public UbisoftScanner(
        ILogService logService)
    {
        _logService = logService;
    }
    
    public Task<List<Game>> ScanAsync() => Task.Run(ScanUbisoft);
    public Game.Platform Platform => Game.Platform.Ubisoft;

    private List<Game> ScanUbisoft()
    {
        var games = new List<Game>();
        if (OperatingSystem.IsWindows())
        {
            games.AddRange(ScanUbisoftLibrary());
        }

        return games;
    }
    
    private List<Game> ScanUbisoftLibrary()
    {
        var games = new List<Game>();

        try
        {
            using var key = Helper.GetOpenRegistryKey(@"\Ubisoft\Launcher\Installs");

            if (key == null) return games;

            foreach (var subName in key.GetSubKeyNames())
            {
                using var gameKey = key.OpenSubKey(subName);
                if (gameKey == null) continue;

                var installDir = Helper.NormalizePath(gameKey.GetValue("InstallDir") as string ?? string.Empty);
                if (string.IsNullOrEmpty(installDir)) continue;
                if (!Directory.Exists(installDir)) continue;

                var name = gameKey.GetValue("Name") as string
                           ?? gameKey.GetValue("DisplayName") as string
                           ?? Path.GetFileName(installDir);
                
                if (string.IsNullOrEmpty(name)) continue;
                
                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = installDir,
                    PlatformName = Platform,
                    PlatformId = $"uplay:{subName}"
                });
            }
        }
        catch
        {
            _logService.LogError($"Could not find Ubisoft games...{games}");
        }


        return games;
    }


}