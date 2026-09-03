using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Restall.Infrastructure.Scanners;

// Ubisoft Scanner Logging - EventId range: 1800 - 1849
internal sealed partial class UbisoftScanner
{
    [LoggerMessage(EventId = 1800, Level = LogLevel.Error,
        Message = "Failed to scan Ubisoft Library [{SubKey}]")]
    private partial void LogFailedToScanUbisoft(string subKey, Exception ex);
    
    [LoggerMessage(EventId = 1801, Level = LogLevel.Debug,
        Message = "Can not find registry root in Ubisoft Scanner, [{SubKey}]")]
    private partial void LogUbisoftRootNotFound(string subKey);
    
    [LoggerMessage(EventId=1802, Level = LogLevel.Debug,
        Message = "Can not find the directory for Ubisoft game [{DisplayName}] [{SubKey}]")]
    private partial void LogUbisoftInstallDirectoryNotFound(string displayName, string subKey);
    [LoggerMessage(EventId=1803, Level = LogLevel.Debug,
        Message = "Display name for [{SubName}] in Ubisoft Scanner is empty ")]
    private partial void LogUbisoftGameDisplayNameEmpty(string subName);
}