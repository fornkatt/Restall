using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Services;

// Game Icon Logging — EventId range: 1000 - 1049
internal sealed partial class GameIconService
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information,
        Message = "Extracted icon for [{GameName}] to [{IconPath}]")]
    private partial void IconExtractionSuccess(string gameName, string iconPath);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Error,
        Message = "Failed to extract icon for [{GameName}]")]
    private partial void IconExtractionFailure(string gameName, Exception ex);
}