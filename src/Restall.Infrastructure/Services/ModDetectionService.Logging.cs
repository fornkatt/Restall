using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Services;

// Mod Detection Logging — EventId range: 1050 - 1099
internal sealed partial class ModDetectionService
{
    [LoggerMessage(EventId = 1050, Level = LogLevel.Debug,
        Message = "Starting {ModType} detection in {Directory}")]
    private partial void LogModDetectionStart(string modType, string directory);

    [LoggerMessage(EventId = 1051, Level = LogLevel.Debug,
        Message = "Finished detection of {ModType} in {Directory}. Found {Count} instances.")]
    private partial void LogModDetectionFinished(string modType, string directory, int count);

    [LoggerMessage(EventId = 1052, Level = LogLevel.Debug,
        Message = "Found {ModType} as {Filename} in {Directory}")]
    private partial void LogModFound(string modType, string filename, string directory);
}