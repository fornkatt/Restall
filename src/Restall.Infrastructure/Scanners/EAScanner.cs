using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate

// TODO(logging-refactor): just swap the logging implementations
internal sealed class EAScanner : IPlatformScannerService
{
    public EAScanner(
    )
    {
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

    //TODO: ADD PUBLISH KEYS HELPER/MANIFEST TO INCLUDE MANY DIFFERENT REGEDITS
    private (List<Game>games, string? error) ScanEALibrary()
    {
        var games = new List<Game>();
        try
        {
            using var key = GameScanHelper.GetOpenRegistryKey(@"\EA Games");

            if (key is null) return (games, null);
#pragma warning disable CA1416 // Already checked before method is called
            foreach (var subName in key.GetSubKeyNames())
            {
                using var gameKey = key.OpenSubKey(subName);
                if (gameKey is null) continue;

                var installDir = GameScanHelper.NormalizePath(
                    GameScanHelper.GetRegistryValue(gameKey, "Install Dir", "InstallLocation", "InstallDir"));

                if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir)) continue;

                var displayName = GameScanHelper.GetRegistryValue(gameKey, "DisplayName")
                                  ?? subName;

                if (string.IsNullOrEmpty(displayName)) continue;

                games.Add(new Game
                {
                    Name = displayName,
                    InstallFolder = installDir,
                    PlatformName = Platform,
                    PlatformId = subName
                });
            }
        }
        catch (Exception ex)
        {
            _logService.LogError("Failed to process EA library", ex);
            return (games, $"Failed to process EA library.");
        }

        return (games, null);
    }
}