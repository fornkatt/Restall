using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IGameLibraryRepository
{
    void SaveAllAsync(ICollection<Game> games);
    void SaveAsync(Game updatedGame);
    void LoadAsync();
}