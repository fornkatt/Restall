using craftersmine.SteamGridDBNet;
using craftersmine.SteamGridDBNet.Exceptions;
using Microsoft.Extensions.Configuration;
using Restall.Application.Helpers;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

internal sealed class SteamGridDbService : ISteamGridDbService
{
    private readonly ILogService _logService;
    private readonly ICachePathService _cachePathService;
    private readonly ISteamGridDbIndexRepository _indexRepository;
    private readonly SteamGridDb? _sgdb;
    private readonly HttpClient _httpClient;

    public SteamGridDbService(
        ILogService logService,
        ICachePathService cacheService,
        ISteamGridDbIndexRepository indexRepository,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logService = logService;
        _cachePathService = cacheService;
        _indexRepository = indexRepository;
        _httpClient = httpClient;

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
        var parts = game.PlatformId!.Split(':', 2);
        if (parts.Length < 2)
        {
            await DownloadByNameSearchAsync(game);
            return;
        }

        var platform = parts[0];
        var id = parts[1];

        var sgdbPlatform = platform switch
        {
            "steam" => (SteamGridDbGamePlatform?)SteamGridDbGamePlatform.Steam,
            "gog" => SteamGridDbGamePlatform.Gog,
            "epic" => SteamGridDbGamePlatform.Egs,
            "uplay" => SteamGridDbGamePlatform.Uplay,
            "origin" => SteamGridDbGamePlatform.Origin,
            _ => null
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
        catch (SteamGridDbNotFoundException ex)
        {
            await _logService.LogErrorAsync($"No SGDB entry for {game.Name} — trying name search", ex);
            await DownloadByNameSearchAsync(game);
        }
    }

    private async Task SetGameImagePathsAsync(
        Game game, int sgdbId, int platformId, SteamGridDbGamePlatform platform)
    {
        var bannerPath = _cachePathService.GetSgdbBannerPath(sgdbId);
        var iconPath = _cachePathService.GetSgdbThumbnailPath(sgdbId);
        var logoPath = _cachePathService.GetSgdbLogoPath(sgdbId);

        Directory.CreateDirectory(Path.GetDirectoryName(bannerPath)!);


        await DownloadBannerIfMissingAsync(sgdbId, bannerPath,
            () => _sgdb!.GetHeroesByPlatformGameIdAsync(platform, platformId));
        await DownloadThumbnailsIfMissingAsync(sgdbId, iconPath,
            () => _sgdb!.GetIconsByPlatformGameIdAsync(platform, platformId));
        await DownloadLogoIfMissingAsync(sgdbId, logoPath,
            () => _sgdb!.GetLogosByPlatformGameIdAsync(platform, platformId));

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
            var bannerPath = _cachePathService.GetSgdbBannerPath(cachedId.Value);
            var iconPath = _cachePathService.GetSgdbThumbnailPath(cachedId.Value);
            var logoPath = _cachePathService.GetSgdbLogoPath(cachedId.Value);

            Directory.CreateDirectory(Path.GetDirectoryName(bannerPath)!);

            await DownloadBannerIfMissingAsync(cachedId.Value, bannerPath,
                () => _sgdb!.GetHeroesByGameIdAsync(cachedId.Value));

            await DownloadThumbnailsIfMissingAsync(cachedId.Value, iconPath,
                () => _sgdb!.GetIconsByGameIdAsync(cachedId.Value));
            await DownloadLogoIfMissingAsync(cachedId.Value, logoPath,
                () => _sgdb!.GetLogosByGameIdAsync(cachedId.Value));

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
                    GameNameHelper.NormalizeName(g.Name)));

            //Full name search
            if (bestMatch is null)
            {
                var strippedTerm =
                    GameNameHelper.NormalizeName(GameNameHelper.StripEditionSuffix(game.Name ?? string.Empty));
                results = await _sgdb!.SearchForGamesAsync(strippedTerm);
                bestMatch = results?.FirstOrDefault(g =>
                    GameNameHelper.FuzzyNameMatch(normalizedName, GameNameHelper.NormalizeName(g.Name)));
            }

            if (bestMatch is null) return;

            await _indexRepository.SaveSteamGridDbIdAsync(cacheKey, bestMatch.Id);

            var bannerPath = _cachePathService.GetSgdbBannerPath(bestMatch.Id);
            var iconPath = _cachePathService.GetSgdbThumbnailPath(bestMatch.Id);
            var logoPath = _cachePathService.GetSgdbLogoPath(bestMatch.Id);

            Directory.CreateDirectory(Path.GetDirectoryName(bannerPath)!);

            await DownloadBannerIfMissingAsync(bestMatch.Id, bannerPath,
                () => _sgdb!.GetHeroesByGameIdAsync(bestMatch.Id));
            await DownloadThumbnailsIfMissingAsync(bestMatch.Id, iconPath,
                () => _sgdb!.GetIconsByGameIdAsync(bestMatch.Id));
            await DownloadLogoIfMissingAsync(bestMatch.Id, logoPath, () => _sgdb.GetLogosByGameIdAsync(bestMatch.Id));

            game.BannerPathString = BannerExists(bestMatch.Id) ? bannerPath : null;
            game.ThumbnailPathString = ThumbnailExists(bestMatch.Id) ? iconPath : null;
            game.LogoPathString = LogoExists(bestMatch.Id) ? logoPath : null;
            return;
        }
        catch (SteamGridDbNotFoundException ex)
        {
            //Double exception because SteamGridDb is being caught separately due to a game might not exist
            await _logService.LogErrorAsync($"No SGDB entry found for '{searchTerm}'", ex);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"SGDB name search failed for '{searchTerm}'", ex);
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
            var bytes = await _httpClient.GetByteArrayAsync(imageUrl);
            await File.WriteAllBytesAsync(savePath, bytes);
            await _logService.LogInfoAsync($"Downloaded {label} to {savePath}");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to download {label}", ex);
        }
    }

    private async Task DownloadBannerIfMissingAsync(int sgdb, string bannerPath,
        Func<Task<SteamGridDbHero[]?>> fetchBanner)
    {
        if (BannerExists(sgdb)) return; // File exists

        var heroes = await fetchBanner(); // File is missing, open the envelope NOW!
        var imageUrl = (heroes?.Where(h => !h.IsNsfw)!). 
            FirstOrDefault(h => h.Format is SteamGridDbFormats.Png || h.Format is SteamGridDbFormats.Jpeg)
            ?.FullImageUrl;
        await DownloadImageAsync(imageUrl, bannerPath, "banner");
    }

    private async Task DownloadThumbnailsIfMissingAsync(int sgdb, string savePath,
        Func<Task<SteamGridDbIcon[]?>> fetchIcons)
    {
        if (ThumbnailExists(sgdb)) return;
        var icons = await fetchIcons();
        var imageUrl = icons?.FirstOrDefault()?.FullImageUrl;
        await DownloadImageAsync(imageUrl, savePath, "icon");
    }

    private async Task DownloadLogoIfMissingAsync(int sgdb, string savePath, Func<Task<SteamGridDbLogo[]?>> fetchLogo)
    {
        if (LogoExists(sgdb)) return;
        var logos = await fetchLogo();
        var imageUrl = logos?.FirstOrDefault()?.FullImageUrl;
        await DownloadImageAsync(imageUrl, savePath, "logo");
    }

    private bool BannerExists(int steamGridDbId) => File.Exists(_cachePathService.GetSgdbBannerPath(steamGridDbId));

    private bool ThumbnailExists(int steamGridDbId) =>
        File.Exists(_cachePathService.GetSgdbThumbnailPath(steamGridDbId));

    private bool LogoExists(int steamGridDbId) => File.Exists(_cachePathService.GetSgdbLogoPath(steamGridDbId));
}