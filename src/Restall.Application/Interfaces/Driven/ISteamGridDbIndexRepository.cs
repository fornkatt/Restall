namespace Restall.Application.Interfaces.Driven;

public interface ISteamGridDbIndexRepository
{
    int? TryGetSteamGridDbId(string cacheKey);
    Task SaveSteamGridDbIdAsync(string cacheKey, int steamGridDbId);
}