using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Scanners;

// Xbox Scanner Logging - EventId range: 1850 - 1899
internal sealed partial class XboxScanner
{
    [LoggerMessage(EventId = 1850, Level = LogLevel.Error,
        Message = "Failed to scan the Xbox library \"{GameDir}\"")]
    private partial void LogXboxScannerFailure(string gameDir, Exception ex);

    [LoggerMessage(EventId = 1851, Level = LogLevel.Debug,
        Message = "Could not find the Game name in \"{GameDir}\"")]
    private partial void LogGameNameNotFound(string gameDir);

    [LoggerMessage(EventId = 1852, Level = LogLevel.Debug,
        Message = "Could not find the content directory \"{SubDir}\" for \"{GameDir}\"")]
    private partial void LogContentDirNotFound(string subDir, string gameDir);

    [LoggerMessage(EventId = 1853, Level = LogLevel.Debug,
        Message = "Could not find the 'Microsoft Game config' for Xbox game \"{SubDir}\" in \"{ConfigPath}\"")]
    private partial void LogMicrosoftGameConfigNotFound(string subDir, string configPath);
}