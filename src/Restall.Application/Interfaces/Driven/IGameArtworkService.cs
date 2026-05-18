using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IGameArtworkService
{
    Task EnrichGameArtworkAsync(Game game);
}