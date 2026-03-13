namespace Restall.Application.Interfaces;

public interface ISteamGridDbCacheService
{
    int? TryGetSteamGridDbId(string cacheKey);
    
    Task SaveSteamGridDbIdAsync(string cacheKey, int steamGridDbId);

    string GetBannerPath(int steamGridDbId);
    string GetThumbnailPath(int steamGridDbId);
    string GetLogoPath(int steamGridDbId);
    bool BannerExists(int steamGridDbId);
    bool ThumbnailExists(int steamGridDbId);
    bool LogoExists(int steamGridDbId);
}