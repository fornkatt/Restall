using Restall.Application.DTOs;
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
    
    
    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanUbisoft);
    public Game.Platform Platform => Game.Platform.Ubisoft;
    
    private GameScanResultDto ScanUbisoft()
    {
        var games = new List<Game>();
        var errors = new List<string>();
        
        if (OperatingSystem.IsWindows())
        {
            var (library, error) = ScanUbisoftLibrary();
            
            games.AddRange(library);
            if(error is not null) errors.Add(error);
        }
        return new GameScanResultDto(
            Platform:     Game.Platform.Ubisoft,
            Games:        games,
            Success:      games.Count > 0,
            Message: errors.Count > 0 ? string.Join("; ", errors) : null);
        
    }
    
    private (List<Game> games, string? error) ScanUbisoftLibrary()
    {
        var games = new List<Game>();

        try
        {
            using var key = GameScanHelper.GetOpenRegistryKey(@"\Ubisoft\Launcher\Installs");

            if (key is null) return (games, null);

            foreach (var subName in key.GetSubKeyNames())
            {
                using var gameKey = key.OpenSubKey(subName);
                if (gameKey is null) continue;

                var installDir = GameScanHelper.NormalizePath(
                    GameScanHelper.GetRegistryValue(gameKey, "InstallDir", "Install Dir"));
                if (string.IsNullOrEmpty(installDir)) continue;
                if (!Directory.Exists(installDir)) continue;

                var name = GameScanHelper.GetRegistryValue(gameKey, "Name", "DisplayName") ?? Path.GetFileName(installDir);
                if (string.IsNullOrEmpty(name)) continue;
                
                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = installDir,
                    PlatformName = Platform,
                    PlatformId = $"uplay:{subName}"
                });
            }
            return (games,null);
        }
        catch(Exception ex)
        {
            return(games, $"Failed Ubisoft scan...{ex.Message}");
        }


    }
    
}