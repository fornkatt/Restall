namespace Restall.Application.Interfaces;

public interface ISteamGridDbIndexRepository
{
    int? TryGetSteamGridDbId(string cacheKey);
    Task SaveSteamGridDbIdAsync(string cacheKey, int steamGridDbId);
}