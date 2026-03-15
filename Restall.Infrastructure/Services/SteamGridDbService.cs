using craftersmine.SteamGridDBNet;
using craftersmine.SteamGridDBNet.Exceptions;
using Microsoft.Extensions.Configuration;
using Restall.Application.Helpers;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

public class SteamGridDbService : ISteamGridDbService
{
    private readonly ILogService _logService;
    private readonly ICachePathService _cacheService;
    private readonly ISteamGridDbIndexRepository _indexRepository;
    private readonly SteamGridDb? _sgdb;

    private const double s_similarityThreshold = 0.6d;

    public SteamGridDbService(
        ILogService logService,
        ICachePathService cacheService,
        ISteamGridDbIndexRepository indexRepository,
        IConfiguration configuration)
    {
        _logService = logService;
        _cacheService = cacheService;
        _indexRepository = indexRepository;

        var apiKey = configuration["SteamGridDBApiKey:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
            _sgdb = new SteamGridDb(apiKey);
    }

    public async Task EnrichGameArtworkAsync(Game game)
    {
        if (_sgdb is null)
        {
            await _logService.LogInfoAsync("SteamGridDB API key not configured — skipping artwork.");
            return;
        }

        try
        {
            await DownloadMissingImagesAsync(game);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to load game artwork for [{game.Name}]", ex);
        }
    }
    

    private async Task DownloadMissingImagesAsync(Game game)
    {
        if (game.PlatformId is not null)
            await DownloadByPlatformIdAsync(game);
        else
            await DownloadByNameSearchAsync(game);
    }

    private async Task DownloadByPlatformIdAsync(Game game)
    {
        var parts    = game.PlatformId!.Split(':', 2);
        var platform = parts[0];
        var id       = parts[1];

        var sgdbPlatform = platform switch
        {
            "steam"  => (SteamGridDbGamePlatform?)SteamGridDbGamePlatform.Steam,
            "gog"    => SteamGridDbGamePlatform.Gog,
            "epic"   => SteamGridDbGamePlatform.Egs,
            "uplay"  => SteamGridDbGamePlatform.Uplay,
            "origin" => SteamGridDbGamePlatform.Origin,
            _        => null
        };
        
        if (sgdbPlatform is null || !int.TryParse(id, out var platformId))
        {
            await DownloadByNameSearchAsync(game);
            return;
        }

        var cacheKey = game.PlatformId;
        var sgdbId = _indexRepository.TryGetSteamGridDbId(cacheKey);

        try
        {
            
            if (sgdbId is not null)
            {
                await SetGameImagePathsAsync(game, sgdbId.Value, platformId, sgdbPlatform.Value);
                return;
            }
            
            var grids = await _sgdb!.GetGridsByPlatformGameIdAsync(sgdbPlatform.Value, platformId);
            var resolvedId = grids?.FirstOrDefault()?.Id;

            if (resolvedId is null)
            {
                await _logService.LogInfoAsync($"No SGDB entry for {game.Name} — trying name search");
                await DownloadByNameSearchAsync(game);
                return;
            }

            await _indexRepository.SaveSteamGridDbIdAsync(cacheKey, resolvedId.Value);
            await _logService.LogInfoAsync($"Resolved SGDB ID for {game.Name} → {resolvedId}");

            await SetGameImagePathsAsync(game, resolvedId.Value, platformId, sgdbPlatform.Value);
        }
        catch (SteamGridDbNotFoundException)
        {
            await _logService.LogInfoAsync($"No SGDB entry for {game.Name} — trying name search");
            await DownloadByNameSearchAsync(game);
        }
    }

    private async Task SetGameImagePathsAsync(
        Game game, int sgdbId, int platformId, SteamGridDbGamePlatform platform)
    {
        var bannerPath = _cacheService.GetSgdbBannerPath(sgdbId);
        var iconPath   = _cacheService.GetSgdbThumbnailPath(sgdbId);
        var logoPath = _cacheService.GetSgdbLogoPath(sgdbId);
        
        var bannerExists = 
        
        Directory.CreateDirectory(Path.GetDirectoryName(bannerPath)!);

        if (!BannerExists(sgdbId))
        {
            var heroes   = await _sgdb!.GetHeroesByPlatformGameIdAsync(platform, platformId);

            var imageUrl = (heroes?.Where(h => !h.IsNsfw)!).
                FirstOrDefault(h => h.Format is SteamGridDbFormats.Png)?.FullImageUrl;
            await DownloadImageAsync(imageUrl, bannerPath, "banner");
        }

        if (!ThumbnailExists(sgdbId))
        {
            var icons    = await _sgdb!.GetIconsByPlatformGameIdAsync(platform, platformId);
            var imageUrl = icons?.FirstOrDefault()?.FullImageUrl;
            await DownloadImageAsync(imageUrl, iconPath, "icon");
        }

        if (!LogoExists(sgdbId))
        {
            var logos = await _sgdb.GetLogosByPlatformGameIdAsync(platform, platformId);
            var imageUrl = logos?.FirstOrDefault()?.FullImageUrl;
            await DownloadImageAsync(imageUrl, logoPath, "logo");
        }
        
        game.BannerPathString = BannerExists(sgdbId) ? bannerPath : null;
        game.ThumbnailPathString = ThumbnailExists(sgdbId) ? iconPath : null;
        game.LogoPathString = LogoExists(sgdbId) ? logoPath : null;
    }

    private async Task DownloadByNameSearchAsync(Game game)
    {
        
        var normalizedName = GameNameHelper.NormalizeName(game.Name ?? string.Empty);
        var searchTerm = normalizedName;
        var cacheKey = $"name:{normalizedName}";
        var cachedId = _indexRepository.TryGetSteamGridDbId(cacheKey);
        
        if (cachedId is not null)
        {
            var bannerPath = _cacheService.GetSgdbBannerPath(cachedId.Value);
            var iconPath   = _cacheService.GetSgdbThumbnailPath(cachedId.Value);
            var logoPath = _cacheService.GetSgdbLogoPath(cachedId.Value);

            Directory.CreateDirectory(Path.GetDirectoryName(bannerPath)!);

            if (!BannerExists(cachedId.Value))
            {
                var heroes   = await _sgdb!.GetHeroesByGameIdAsync(cachedId.Value);
                var imageUrl = heroes?.FirstOrDefault()?.FullImageUrl;
                await DownloadImageAsync(imageUrl, bannerPath, "banner");
            }

            if (!ThumbnailExists(cachedId.Value))
            {
                var icons    = await _sgdb!.GetIconsByGameIdAsync(cachedId.Value);
                var imageUrl = icons?.FirstOrDefault()?.FullImageUrl;
                await DownloadImageAsync(imageUrl, iconPath, "icon");
            }

            if (!LogoExists(cachedId.Value))
            {
                var logos  = await _sgdb.GetLogosByGameIdAsync(cachedId.Value);
                var imageUrl = logos?.FirstOrDefault()?.FullImageUrl;
                await DownloadImageAsync(imageUrl,logoPath, "logo");
            }
            
            game.BannerPathString = BannerExists(cachedId.Value) ? bannerPath : null;
            game.ThumbnailPathString = ThumbnailExists(cachedId.Value) ? iconPath : null;
            game.LogoPathString = LogoExists(cachedId.Value) ? logoPath : null;
            return;
        }
        
        
        try
        {
                var results = await _sgdb!.SearchForGamesAsync(searchTerm);
                if (results is null || results.Length == 0) return;

                var bestMatch = results.FirstOrDefault(g =>
                    GameNameHelper.FuzzyNameMatch(
                        normalizedName,
                        GameNameHelper.NormalizeName(g.Name)));;

                if (bestMatch is null)
                {
                     var strippedTerm = GameNameHelper.NormalizeName(GameNameHelper.StripEditionSuffix(game.Name ?? string.Empty));
                     results = await _sgdb!.SearchForGamesAsync(strippedTerm);
                     bestMatch = results?.FirstOrDefault(g => GameNameHelper.FuzzyNameMatch(normalizedName, GameNameHelper.NormalizeName(g.Name)));
                }
                
                
                await _indexRepository.SaveSteamGridDbIdAsync(cacheKey, bestMatch.Id);
        
                var bannerPath = _cacheService.GetSgdbBannerPath(bestMatch.Id);
                var iconPath   = _cacheService.GetSgdbThumbnailPath(bestMatch.Id);
                var logoPath = _cacheService.GetSgdbLogoPath(bestMatch.Id);
        
                Directory.CreateDirectory(Path.GetDirectoryName(bannerPath)!);
        
                if (!BannerExists(bestMatch.Id))
                {
                    var heroes   = await _sgdb!.GetHeroesByGameIdAsync(bestMatch.Id);
                    var imageUrl = heroes?.FirstOrDefault()?.FullImageUrl;
                    await DownloadImageAsync(imageUrl, bannerPath, "banner");
                }
        
                if (!ThumbnailExists(bestMatch.Id))
                {
                    var icons    = await _sgdb!.GetIconsByGameIdAsync(bestMatch.Id);
                    var imageUrl = icons?.FirstOrDefault()?.FullImageUrl;
                    await DownloadImageAsync(imageUrl, iconPath, "icon");
                }

                if (!LogoExists(bestMatch.Id))
                {
                    var logos   = await _sgdb.GetLogosByGameIdAsync(bestMatch.Id);
                    var imageUrl = logos?.FirstOrDefault()?.FullImageUrl;
                    await DownloadImageAsync(imageUrl, logoPath, "logo");
                }
                
                game.BannerPathString = BannerExists(bestMatch.Id) ? bannerPath : null;
                game.ThumbnailPathString = ThumbnailExists(bestMatch.Id) ? iconPath : null;
                game.LogoPathString = LogoExists(bestMatch.Id) ? logoPath : null;
                return;
        }
        catch (Exception ex)
        {
            await _logService.LogWarningAsync($"SGDB name search failed for '{searchTerm}': {ex.Message}");
        }
        
        await _logService.LogWarningAsync($"No SGDB match found for {game.Name}");
    }

    private async Task DownloadImageAsync(string? imageUrl, string savePath, string label)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            await _logService.LogWarningAsync($"No {label} URL found");
            return;
        }

        try
        {
            using var http = new HttpClient();
            var bytes      = await http.GetByteArrayAsync(imageUrl);
            await File.WriteAllBytesAsync(savePath, bytes);
            await _logService.LogInfoAsync($"Downloaded {label} to {savePath}");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to download {label}", ex);
        }
    }
    
    private bool BannerExists(int steamGridDbId) => File.Exists(_cacheService.GetSgdbBannerPath(steamGridDbId));
    private bool ThumbnailExists(int steamGridDbId) => File.Exists(_cacheService.GetSgdbThumbnailPath(steamGridDbId));
    private bool LogoExists(int steamGridDbId) => File.Exists(_cacheService.GetSgdbLogoPath(steamGridDbId));
}