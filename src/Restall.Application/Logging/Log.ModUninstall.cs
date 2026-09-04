using Microsoft.Extensions.Logging;

namespace Restall.Application.Logging;

// Mod Uninstall Failures Logging — EventId range: 0 - 24
public static partial class Log
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Error,
        Message = "Failed to uninstall {ModType} from {GameName} — Service returned: {ErrorMessage}")]
    public static partial void ModUninstallFailure(this ILogger logger, string modType, string gameName,
        string? errorMessage, Exception? ex);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message =
            "Uninstalling {ModType} from [{GameName}] — {GameExecutableDirectory}")]
    public static partial void ModUninstallationStart(this ILogger logger, string modType, string gameName,
        string gameExecutableDirectory);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "Finished uninstalling {ModType} from [{GameName}]")]
    public static partial void ModUninstallationComplete(this ILogger logger, string modType, string gameName);
}