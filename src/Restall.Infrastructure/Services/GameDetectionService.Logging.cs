using Microsoft.Extensions.Logging;


namespace Restall.Infrastructure.Services;

// Game Detection Service Logging - EventId range: 1550 - 1599
internal sealed partial class GameDetectionService
{
    [LoggerMessage(EventId = 1550, Level = LogLevel.Error,
        Message = "Fatal error when scanning libraries. Aborting.")]
    private partial void LogFailedToScanLibraries(Exception ex);
    [LoggerMessage(EventId = 1551, Level = LogLevel.Debug,
    Message = "[{Platform}] scanner finished, {Count} games found")]
    private partial void LogScannerFinished(string platform, int count);
    
    [LoggerMessage(EventId = 1552, Level = LogLevel.Error,
        Message = "Skipping [{GameName}] at [{InstallFolder}] because of engine detection failure")]
    private partial void LogFailedToDetectExecutablePathAndEngine(string gameName, string installFolder, Exception ex);

}