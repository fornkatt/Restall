using System.Xml.Linq;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Scanners;

internal sealed class XboxScanner : IPlatformScannerService
{
    private readonly ILogService _logService;
    
    public XboxScanner(ILogService logService)
    {
        _logService = logService;
    }

    public Task<GameScanResultDto> ScanAsync() => Task.Run(ScanXbox);
    public Game.Platform Platform =>  Game.Platform.Xbox;
    private GameScanResultDto ScanXbox()
    {
        var games = new List<Game>();
        var errors = new List<string>();
        
        foreach (var xboxPath in GetXboxInstallPaths()) 
        {
            var (library, error) = ScanXboxLibrary(xboxPath);
            games.AddRange(library);
            if(error is not null) errors.Add(error);
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
            try
            {
                var contentDir = Path.Combine(gameDir, "Content");
                if (!Directory.Exists(contentDir)) continue;

                var configPath = Path.Combine(gameDir, "Content", "MicrosoftGame.config");
                if (!File.Exists(configPath)) continue;

                var (name, storeId, iconFile) = ParseMicrosoftGameConfig(configPath);

                var resolvedName = name ?? Path.GetFileName(gameDir);

                var iconPath = iconFile is not null ? Path.Combine(gameDir, "Content", iconFile) : null;

                if (string.IsNullOrWhiteSpace(resolvedName)) continue;

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
                _logService.LogError("Failed to scan Xbox Game Library", ex);
                return (games, "Failed to scan Xbox Game Library");
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