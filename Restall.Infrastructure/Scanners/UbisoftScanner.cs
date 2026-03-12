using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

public class UbisoftScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    private readonly IEngineDetectionService _engineDetectionService;
    

    public UbisoftScanner(
        ILogService logService,
        IEngineDetectionService engineDetectionService)
    {
        _logService = logService;
        _engineDetectionService = engineDetectionService;
    }
    
    public Task<List<Game>> ScanAsync() => Task.Run(ScanUbisoft);

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
                    PlatformName = Game.Platform.Ubisoft
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