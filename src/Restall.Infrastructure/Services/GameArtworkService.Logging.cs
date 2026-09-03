using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Services;

// Game Artwork Service Logging - EventId range: 1500 - 1549
internal sealed partial class GameArtworkService
{
    [LoggerMessage(EventId = 1500, Level = LogLevel.Error,
        Message = "Failed to enrich game artwork for [{GameName}]")]
    private partial void LogFailedToEnrichGameArtwork(string gameName, Exception ex);
}