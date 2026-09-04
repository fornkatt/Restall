using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate

// TODO(logging-refactor): just swap the logging implementations
internal sealed partial class EAScanner : IPlatformScannerService
{
    private readonly ILogger<EAScanner> _logger;

    public EAScanner(
        ILogger<EAScanner> logger
    )
    {
        _logger = logger;
    }

    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanEA);
    public Game.Platform Platform => Game.Platform.EA;

    private GameScanResultDto ScanEA()
    {
        var games = new List<Game>();
        var errors = new List<string>();
        if (OperatingSystem.IsWindows())
        {
            var (library, error) = ScanEALibrary();
            games.AddRange(library);
            if (error is not null) errors.Add(error);
        }

        return new GameScanResultDto(
            Platform: Game.Platform.EA,
            Games: games,
            IsSuccess: games.Count > 0,
            Message: errors.Count > 0 ? string.Join(", ", errors) : null);
    }

    //TODO: SEPARATE SCANLIBRARY FOR EA, GOG AND UBISOFT. ALSO INCLUDE MANY REGISTRY KEYS THROUGH MANIFEST
    private (List<Game>games, string? error) ScanEALibrary()
    {
        var games = new List<Game>();
        
        
        using var key = GameScanHelper.GetOpenRegistryKey(@"\EA Games");
        //TODO: DO LOGGING FOR EASY DETECTION TO IMPLEMENT REGISTRY KEYS THROUGH MANIFEST
        if (key is null) return (games, null);
#pragma warning disable CA1416 // Already checked before method is called
        foreach (var subName in key.GetSubKeyNames())
        {
            try
            {
                using var gameKey = key.OpenSubKey(subName);
                if (gameKey is null) continue;
                
                var displayName = GameScanHelper.GetRegistryValue(gameKey, "DisplayName")
                                  ?? subName;

                if (string.IsNullOrEmpty(displayName))
                {
                    LogEAGameDisplayNameEmpty(subName);
                    continue;
                }
                
                var installDir = GameScanHelper.NormalizePath(
                    GameScanHelper.GetRegistryValue(gameKey, "Install Dir", "InstallLocation", "InstallDir"));

                if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir))
                {
                    LogEAInstallDirectoryNotFound(displayName, subName);
                    continue;
                }
                
                games.Add(new Game
                {
                    Name = displayName,
                    InstallFolder = installDir,
                    PlatformName = Platform,
                    PlatformId = subName
                });
            }
            catch (Exception ex)
            {
                LogEAScannerFailed(subName, ex);
            }
        }
        
        return (games, null);
    }
}