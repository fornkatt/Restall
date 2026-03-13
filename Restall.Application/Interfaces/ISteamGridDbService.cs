using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface ISteamGridDbService
{
    Task EnrichGameArtworkAsync(Game game);
}