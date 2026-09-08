using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Restall.Infrastructure.Scanners;

// GOG Scanner Logging - EventId range: 1700 - 1749
internal sealed partial class GOGScanner
{
    [LoggerMessage(EventId = 1700, Level = LogLevel.Error,
        Message = "Failed to scan the GOG Galaxy library \"{SubKey}\"")]
    private partial void LogGOGLibraryScanFailure(string subKey, Exception ex);

    [LoggerMessage(EventId = 1701, Level = LogLevel.Warning,
        Message = "Could not find the directory for GOG game \"{Name}\" in \"{SubKey}\"")]
    private partial void LogGOGInstallPathNotFound(string name, string subKey);

    [LoggerMessage(EventId = 1702, Level = LogLevel.Warning,
        Message = "Display name for \"{SubName}\" in GOG Scanner is empty")]
    private partial void LogGOGGameDisplayNameEmpty(string subName);
}