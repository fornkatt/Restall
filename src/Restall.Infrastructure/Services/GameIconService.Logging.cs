using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Services;

// Game Icon Logging — EventId range: 1000 - 1049
internal sealed partial class GameIconService
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information,
        Message = "Extracted icon for the game [{GameName}] to [{IconPath}]")]
    private partial void LogIconExtractionSuccess(string gameName, string iconPath);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Error,
        Message = "Failed to extract icon for the game: [{GameName}]")]
    private partial void LogIconExtractionFailure(string gameName, Exception ex);
}