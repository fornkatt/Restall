using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Restall.Infrastructure.Scanners;

// Ubisoft Scanner Logging - EventId range: 1800 - 1849
internal sealed partial class UbisoftScanner
{
    [LoggerMessage(EventId = 1800, Level = LogLevel.Error,
        Message = "Failed to scan Ubisoft library: \"{SubKey}\"")]
    private partial void LogUbisoftScanFailure(string subKey, Exception ex);

    [LoggerMessage(EventId = 1801, Level = LogLevel.Debug,
        Message = "Could not find registry root in Ubisoft Scanner, \"{SubKey}\"")]
    private partial void LogUbisoftRootNotFound(string subKey);

    [LoggerMessage(EventId = 1802, Level = LogLevel.Warning,
        Message =
            "Could not find the directory for Ubisoft game with name \"{DisplayName}\" in subkey \"{SubKey}\"")]
    private partial void LogUbisoftInstallDirectoryNotFound(string displayName, string subKey);

    [LoggerMessage(EventId = 1803, Level = LogLevel.Warning,
        Message = "Display name for \"{SubName}\" in Ubisoft Scanner is empty")]
    private partial void LogUbisoftGameDisplayNameEmpty(string subName);
}