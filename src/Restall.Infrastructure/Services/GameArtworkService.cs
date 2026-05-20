using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;



namespace Restall.Infrastructure.Services;

internal sealed class GameArtworkService : IGameArtworkService
{
    private readonly ILogService _logService;
    private readonly IPathService _pathService;
    private readonly IGameCoverService _gameCoverService;
    private readonly IGameIconService _gameIconService;
    private readonly IImageResizeService _imageResizeService;

    public GameArtworkService(ILogService logService,
        IPathService pathService,
        IGameCoverService gameCoverService,
        IGameIconService gameIconService
        , IImageResizeService imageResizeService)
    {
        _logService = logService;
        _pathService = pathService;
        _gameCoverService = gameCoverService;
        _gameIconService = gameIconService;
        _imageResizeService = imageResizeService;

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
                if(game.ThumbnailPathString is not null && File.Exists(game.ThumbnailPathString))
                    File.Copy(game.ThumbnailPathString, iconPath, overwrite: true);
                else 
                    await _gameIconService.ExtractIconIfMissingAsync(game.ExecutablePath, game.Name, iconPath);
            }
            
            game.GameCoverPathString = File.Exists(coverPath) ? coverPath : string.Empty;
            game.ThumbnailPathString = File.Exists(iconPath) ? iconPath : string.Empty;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to enrich game artwork for [{game.Name}]", ex);
        }
    }
}