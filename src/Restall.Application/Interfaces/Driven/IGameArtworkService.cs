namespace Restall.Application.Interfaces.Driven;

public interface IGameArtworkService
{
    Task EnrichGameArtworkAsync(string slug);
}