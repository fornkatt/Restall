using Microsoft.Extensions.Logging;
using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;


namespace Restall.Infrastructure.Services;

// TODO: surface Result/Result<T> in applicable methods. Use ErrorType, log at call-site if appropriate

// TODO(logging-refactor): just swap the logging implementations
internal sealed partial class GameArtworkService : IGameArtworkService
{
    private readonly IPathService _pathService;
    private readonly IGameCoverService _gameCoverService;
    private readonly IGameIconService _gameIconService;
    private readonly ILogger<GameArtworkService> _logger;

    public GameArtworkService(
        IPathService pathService,
        IGameCoverService gameCoverService,
        IGameIconService gameIconService,
        ILogger<GameArtworkService> logger)
    {
        _pathService = pathService;
        _gameCoverService = gameCoverService;
        _gameIconService = gameIconService;
        _logger = logger;
        
        Directory.CreateDirectory(pathService.GetArtworkCacheDirectory());
    }
    
    public async Task EnrichGameArtworkAsync(Game game)
    {
        try
        {
            var slug = GameNameHelper.NormalizeName(game.Name ?? string.Empty);
            var coverPath = _pathService.GetGameArtworkCover(slug);
            var iconPath = _pathService.GetGameArtThumbnailPath(slug);

            Directory.CreateDirectory(Path.GetDirectoryName(coverPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(iconPath)!);

            await _gameCoverService.DownloadCoverIfMissingAsync(game, coverPath);

            if (!File.Exists(iconPath))
            {
                if (game.ThumbnailPathString is not null && File.Exists(game.ThumbnailPathString))
                    File.Copy(game.ThumbnailPathString, iconPath, overwrite: true);
                else
                    await _gameIconService.ExtractIconIfMissingAsync(game.ExecutablePath, game.Name, iconPath);
            }

            game.GameCoverPathString = File.Exists(coverPath) ? coverPath : string.Empty;
            game.ThumbnailPathString = File.Exists(iconPath) ? iconPath : string.Empty;
        }
        catch (Exception ex)
        {
            LogFailedToEnrichGameArtwork(game.Name ?? "Unknown", ex);
        }
    }
}