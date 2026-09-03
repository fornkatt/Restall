using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Scanners;

// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate

// TODO(logging-refactor): just swap the logging implementations
internal sealed partial class XboxScanner : IPlatformScannerService
{
    private readonly ILogger<XboxScanner> _logger;
    
    public XboxScanner(
        ILogger<XboxScanner> logger
    )
    {
        _logger = logger;
    }

    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanXbox);
    public Game.Platform Platform => Game.Platform.Xbox;

    private GameScanResultDto ScanXbox()
    {
        var games = new List<Game>();
        var errors = new List<string>();

        foreach (var xboxPath in GetXboxInstallPaths())
        {
            var (library, error) = ScanXboxLibrary(xboxPath);
            games.AddRange(library);
            if (error is not null) errors.Add(error);
        }

        return new GameScanResultDto(
            Platform: Game.Platform.Xbox,
            Games: games,
            IsSuccess: games.Count > 0,
            Message: errors.Count > 0 ? string.Join(", ", errors) : null);
    }

    private (List<Game> games, string? error) ScanXboxLibrary(string installPath)
    {
        var games = new List<Game>();

        foreach (var gameDir in Directory.EnumerateDirectories(installPath))
        {
            var subDir = Path.GetFileName(gameDir);
            
            try
            {
                var contentDir = Path.Combine(gameDir, "Content");
                
                if (!Directory.Exists(contentDir))
                {
                    
                    LogContentDirNotFound(subDir, gameDir);
                    continue;
                }
                
                var configPath = Path.Combine(gameDir, "Content", "MicrosoftGame.config");
                
                if (!File.Exists(configPath))
                {
                    LogMicrosoftGameConfigNotFound(subDir, configPath);
                    continue;
                }
                
                var (name, storeId, iconFile) = ParseMicrosoftGameConfig(configPath);
                var resolvedName = name ?? Path.GetFileName(gameDir);
                
                if (string.IsNullOrWhiteSpace(resolvedName))
                {
                    LogGameNameNotFound(gameDir);
                    continue;
                }
                
                var iconPath = iconFile is not null ? Path.Combine(gameDir, "Content", iconFile) : null;
                
                games.Add(new Game
                {
                    Name = resolvedName,
                    InstallFolder = gameDir,
                    ExecutablePath = contentDir,
                    ThumbnailPathString = File.Exists(iconPath) ? iconPath : null,
                    PlatformName = Platform,
                    PlatformId = storeId
                });
            }
            catch (Exception ex)
            {
                LogXboxScannerFailed(gameDir, ex);
                
            }
        }

        return (games, null);
    }


    private static (string? name, string? storeId, string? iconFile) ParseMicrosoftGameConfig(string configPath)
    {
        var document = XDocument.Load(configPath);
        var nameSpace = document.Root?.Name.Namespace ?? XNamespace.None;
        var shellVisuals = document.Root?.Element(nameSpace + "ShellVisuals");

        var name = shellVisuals?.Attribute("DefaultDisplayName")?.Value;
        var iconFile = shellVisuals?.Attribute("Square44x44Logo")?.Value;

        var storeId = document.Root?.Element(nameSpace + "StoreId")?.Value;

        return (name, storeId, iconFile);
    }

    private static List<string> GetXboxInstallPaths() =>
        DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Fixed && d.IsReady)
            .Select(d => Path.Combine(d.RootDirectory.FullName, "XboxGames"))
            .Where(Directory.Exists).ToList();
}