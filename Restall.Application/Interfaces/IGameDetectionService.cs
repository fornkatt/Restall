using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IGameDetectionService
{
    Task<List<Game?>> FindGames();
}