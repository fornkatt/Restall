using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface ISteamGridDbService
{
    Task EnrichGameArtworkAsync(Game game);
}