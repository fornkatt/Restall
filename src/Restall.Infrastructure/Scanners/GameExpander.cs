using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Scanners;

internal static class GameExpander
{
    internal static IEnumerable<Game> ExpandCollection(Game game)
    {
        if (GameScanHelper.IsMassEffectLegendary(game.Name))
        {
            var meFolders = new[] {"ME1", "ME2", "ME3"};
            return meFolders.Select(me => new Game
            {
                Name = $"{game.Name} - {me}",
                InstallFolder = game.InstallFolder,
                ExecutablePath = Path.Combine(game.InstallFolder!, "Game", me),
                ThumbnailPathString = game.ThumbnailPathString,
                PlatformName = game.PlatformName,
                PlatformId = game.PlatformId,
            });

        }
        
        return [game];
    }
}