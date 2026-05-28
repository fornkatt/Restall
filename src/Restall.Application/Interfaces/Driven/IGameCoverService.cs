using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IGameCoverService
{
    Task DownloadCoverIfMissingAsync(Game game, string coverPath);
}