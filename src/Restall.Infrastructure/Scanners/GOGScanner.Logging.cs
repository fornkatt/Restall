using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Restall.Infrastructure.Scanners;

// GOG Scanner Logging - EventId range: 1700 - 1749
internal sealed partial class GOGScanner
{
    [LoggerMessage(EventId = 1700, Level = LogLevel.Error,
        Message = "Failed to scan the GOG Galaxy library [{Error}]")]
    private partial void LogFailedToScanGOGLibrary(string error, Exception ex);
    
    [LoggerMessage(EventId = 1701, Level = LogLevel.Debug,
        Message = "Can not find the directory for GOG game [{Name}] in [{SubKey}]")]
    private partial void LogGOGInstallPathNotFound(string name, string subKey);
    
    [LoggerMessage(EventId=1702, Level = LogLevel.Debug,
        Message = "Display name for [{SubName}] in GOG Scanner is empty")]
    private partial void LogGOGGameDisplayNameEmpty(string subName);
    
    [LoggerMessage(EventId = 1703, Level = LogLevel.Error,
        Message = "Failed to read installed.json file [{installedJsonPath}] in GOG Heroic library")]
    private partial void LogGOGHeroicFailedToReadJsonFile(string installedJsonPath, Exception ex);
    
    [LoggerMessage(EventId=1704, Level = LogLevel.Error,
        Message="Failed to scan the json block [{json}] in GOG Heroic library")]
    private partial void LogGOGHeroicFailedToScanJsonBlock(string json, Exception ex);
    
    [LoggerMessage(EventId=1705, Level = LogLevel.Debug,
        Message="Failed to find the install path for GOG Heroic game with AppName: [{appName}] ")]
    private partial void LogGOGHeroicFailedToFindInstallPath(string? appName);
    
    [LoggerMessage(EventId=1706, Level = LogLevel.Debug,
        Message = "Failed to find the name for GOG Heroic Game with [{AppName}] and install path: [{InstallPath}]")]
    private partial void LogGOGHeroicFailedToFindName(string? appName, string installPath);

}