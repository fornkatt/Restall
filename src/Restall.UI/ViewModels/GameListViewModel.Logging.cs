using System;
using Microsoft.Extensions.Logging;

namespace Restall.UI.ViewModels;

// Game List Refresh Logging — EventId range: 1200 - 1249
public sealed partial class GameListViewModel
{
    [LoggerMessage(EventId = 1200, Level = LogLevel.Information,
        Message = "Full library refresh started")]
    private partial void LogFullLibraryRefreshStart();

    [LoggerMessage(EventId = 1201, Level = LogLevel.Error,
        Message = "Library refresh failed with message: {ErrorMessage}")]
    private partial void LogLibraryRefreshFailure(string errorMessage);

    [LoggerMessage(EventId = 1202, Level = LogLevel.Information,
        Message = "Full library refresh completed")]
    private partial void LogFullLibraryRefreshCompleted();

    [LoggerMessage(EventId = 1203, Level = LogLevel.Information,
        Message = "Light game refresh started")]
    private partial void LogLightGameRefreshStart();

    [LoggerMessage(EventId = 1204, Level = LogLevel.Information,
        Message = "Light game refresh completed")]
    private partial void LogLightGameRefreshComplete();

    [LoggerMessage(EventId = 1205, Level = LogLevel.Information,
        Message = "Refresh cancelled by user")]
    private partial void LogRefreshCancelled(Exception ex);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error,
        Message = "An unexpected error occurred during refresh")]
    private partial void LogRefreshFailure(Exception ex);
}