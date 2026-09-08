using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Scanners;

// EA Scanner Logging — EventId range: 1600 - 1649
internal sealed partial class EAScanner
{
    [LoggerMessage(EventId = 1600, Level = LogLevel.Error,
        Message = "Failed to scan EA library: \"{SubKey}\"")]
    private partial void LogEAScanFailure(string subKey, Exception ex);

    [LoggerMessage(EventId = 1601, Level = LogLevel.Warning,
        Message = "Could not find the directory for the game from EA App: \"{DisplayName}\" in \"{SubKey}\"")]
    private partial void LogEAInstallDirectoryNotFound(string displayName, string subKey);

    [LoggerMessage(EventId = 1602, Level = LogLevel.Warning,
        Message = "Display name for \"{SubName}\" in EA Scanner is empty")]
    private partial void LogEAGameDisplayNameEmpty(string subName);
}