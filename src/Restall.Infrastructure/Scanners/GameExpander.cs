using Restall.Application.Helpers;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Scanners;

internal static class GameExpander
{
    internal static IEnumerable<Game> ExpandCollection(Game game)
    {
        var collection = GameCollectionCatalog.Find(game.Name);

        if (collection is null)
            return [game];

        return collection.Parts.Select(part => new Game
        {
            Name = $"{game.Name} - {part.DisplaySuffix}",
            InstallFolder = game.InstallFolder,
            ExecutablePath = collection.BuildExecutablePath(game.InstallFolder!, part),
            ThumbnailPathString = game.ThumbnailPathString,
            PlatformName = game.PlatformName,
            PlatformId = game.PlatformId,
        });
    }
}