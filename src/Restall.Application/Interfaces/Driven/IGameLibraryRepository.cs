using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IGameLibraryRepository
{
    void SaveAllAsync(ICollection<Game> games);
    void SaveAsync(Game updatedGame);
    void LoadAsync();
}