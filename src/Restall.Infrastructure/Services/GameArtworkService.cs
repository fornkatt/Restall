using System.Text.Json;
using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;


namespace Restall.Infrastructure.Services;

internal sealed class GameArtworkService : IGameArtworkService
{
    private readonly ILogService _logService;
    private readonly IPathService _pathService;
    private readonly IGameCoverService _gameCoverService;
    private readonly IGameIconService _gameIconService;


    public GameArtworkService(ILogService logService,
        IPathService pathService,
        IGameCoverService gameCoverService,
        IGameIconService gameIconService)
    {
        _logService = logService;
        _pathService = pathService;
        _gameCoverService = gameCoverService;
        _gameIconService = gameIconService;


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
            await _gameIconService.ExtractIconIfMissingAsync(game.ExecutablePath, game.Name, iconPath);

            game.GameCoverPathString = File.Exists(coverPath) ? coverPath : string.Empty;
            game.ThumbnailPathString = File.Exists(iconPath) ? iconPath : string.Empty;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to enrich game artwork for [{game.Name}]", ex);
        }
    }
}