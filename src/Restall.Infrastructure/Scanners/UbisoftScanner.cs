using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate

// TODO(logging-refactor): just swap the logging implementations
internal sealed partial class UbisoftScanner : IPlatformScannerService
{
    private readonly ILogger<UbisoftScanner> _logger;

    public UbisoftScanner(
        ILogger<UbisoftScanner> logger
    )
    {
        _logger = logger;
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
            if (error is not null) errors.Add(error);
        }

        return new GameScanResultDto(
            Platform: Game.Platform.Ubisoft,
            Games: games,
            IsSuccess: games.Count > 0,
            Message: errors.Count > 0 ? string.Join(", ", errors) : null);
    }

    private (List<Game> games, string? error) ScanUbisoftLibrary()
    {
        var games = new List<Game>();

        using var key = GameScanHelper.GetOpenRegistryKey(@"\Ubisoft\Launcher\Installs");
        if (key is null) return (games, null);

#pragma warning disable CA1416 // Handled before method is called
        foreach (var subName in key.GetSubKeyNames())
        {
            try
            {
                using var gameKey = key.OpenSubKey(subName);
                if (gameKey is null) continue;
                

                var installDir = GameScanHelper.NormalizePath(
                    GameScanHelper.GetRegistryValue(gameKey, "InstallDir", "Install Dir"));
                var name = GameScanHelper.GetRegistryValue(gameKey, "Name", "DisplayName") ??
                    Path.GetFileName(installDir);
                
                if (string.IsNullOrEmpty(installDir))
                {
                    LogUbisoftInstallDirectoryNotFound(name ?? "Unknown Game", subName);
                    continue;
                }
                
                if (string.IsNullOrEmpty(name))
                {
                    LogUbisoftGameDisplayNameEmpty(subName);
                    continue;
                }

                games.Add(new Game
                {
                    Name = name,
                    InstallFolder = installDir,
                    PlatformName = Platform,
                    PlatformId = subName
                });
            }
            catch (Exception ex)
            {
                LogUbisoftScannerFailed(subName, ex);
                
            }
        }
        
        return (games, null);
    }
}