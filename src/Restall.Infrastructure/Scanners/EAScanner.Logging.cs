using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Restall.Infrastructure.Scanners;

// EA Scanner Logging - EventId range: 1600 - 1649
internal sealed partial class EAScanner
{
    [LoggerMessage(EventId = 1600, Level = LogLevel.Error,
    Message = "Failed to scan the EA library: [{SubKey}]")]
    private partial void LogFailedToScanEA(string subKey, Exception ex);
    
    [LoggerMessage(EventId=1601, Level = LogLevel.Debug,
        Message = "Can not find the directory for the game from EA App: [{DisplayName}] in [{SubKey}]")]
    private partial void LogEAInstallDirectoryNotFound(string displayName, string subKey);
    
    [LoggerMessage(EventId=1602, Level = LogLevel.Debug,
        Message = "Display name for [{SubName}] in EA Scanner is empty")]
    private partial void LogEAGameDisplayNameEmpty(string subName);
    
}