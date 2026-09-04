using Microsoft.Extensions.Logging;

namespace Restall.Application.Logging;

// Mod Install Failures Logging — EventId range: 25 - 49
public static partial class Log
{
    [LoggerMessage(EventId = 25, Level = LogLevel.Error,
        Message = "Failed to download {ModType} to cache ({CacheDirectory}) — Service returned: {ErrorMessage}")]
    public static partial void ModDownloadFailure(this ILogger logger, string modType, string cacheDirectory,
        string? errorMessage, Exception? ex);

    [LoggerMessage(EventId = 26, Level = LogLevel.Error,
        Message = "Failed to extract files from {ModType} installer to {CacheDirectory} " +
                  "— Service returned: {ErrorMessage}")]
    public static partial void ModExtractionFailure(this ILogger logger, string modType, string cacheDirectory,
        string? errorMessage, Exception? ex);

    [LoggerMessage(EventId = 27, Level = LogLevel.Error,
        Message = "Failed to delete existing {ModType} file on {GameName} — Service returned: {ErrorMessage}")]
    public static partial void ExistingModFileDeletionFailure(this ILogger logger, string modType, string gameName,
        string? errorMessage, Exception? ex);

    [LoggerMessage(EventId = 28, Level = LogLevel.Error,
        Message = "Failed to install {ModType} to {GameName} — Service returned: {ErrorMessage}")]
    public static partial void ModInstallationFailure(this ILogger logger, string modType, string gameName,
        string? errorMessage, Exception? ex);

    [LoggerMessage(EventId = 29, Level = LogLevel.Information,
        Message =
            "Starting installation of {ModType} as {ModName} [{Arch}] to [{GameName}] — {GameExecutableDirectory}")]
    public static partial void ModInstallationStart(this ILogger logger, string modType, string modName, string arch,
        string gameName, string gameExecutableDirectory);

    [LoggerMessage(EventId = 30, Level = LogLevel.Information,
        Message = "Finished installation of {ModType} as {ModName} [{Arch}] to [{GameName}]")]
    public static partial void ModInstallationComplete(this ILogger logger, string modType, string modName, string arch,
        string gameName);
}