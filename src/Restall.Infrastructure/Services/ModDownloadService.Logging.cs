using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Services;

// Mod Download Logging — EventId range: 1150 - 1199
internal sealed partial class ModDownloadService
{
    [LoggerMessage(EventId = 1150, Level = LogLevel.Information,
        Message = "{Filename} already exists at destination. Skipping.")]
    private partial void LogFileAlreadyExists(string filename);

    [LoggerMessage(EventId = 1151, Level = LogLevel.Information,
        Message = "Beginning download of {Filename} to {DestinationDirectory} from {Url}")]
    private partial void LogFileDownloadStart(string filename, string? destinationDirectory, string url);

    [LoggerMessage(EventId = 1152, Level = LogLevel.Information,
        Message = "Successfully downloaded {Filename} to {DestinationDirectory}")]
    private partial void LogFileDownloadSuccess(string filename, string? destinationDirectory);

    [LoggerMessage(EventId = 1153, Level = LogLevel.Error,
        Message = "Failed to clean up partial download at {DestinationPath}")]
    private partial void LogPartialDownloadCleanupFailure(string destinationPath, Exception ex);
}