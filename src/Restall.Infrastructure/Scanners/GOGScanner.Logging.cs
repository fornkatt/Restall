using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Restall.Infrastructure.Scanners;

// GOG Scanner Logging - EventId range: 1700 - 1749
internal sealed partial class GOGScanner
{
    [LoggerMessage(EventId = 1700, Level = LogLevel.Error,
        Message = "Failed to scan the GOG Galaxy library \"{Error}\"")]
    private partial void LogGOGLibraryScanFailed(string error, Exception ex);

    [LoggerMessage(EventId = 1701, Level = LogLevel.Debug,
        Message = "Can not find the directory for GOG game \"{Name}\" in \"{SubKey}\"")]
    private partial void LogGOGInstallPathNotFound(string name, string subKey);

    [LoggerMessage(EventId = 1702, Level = LogLevel.Debug,
        Message = "Display name for \"{SubName}\" in GOG Scanner is empty")]
    private partial void LogGOGGameDisplayNameEmpty(string subName);

    [LoggerMessage(EventId = 1703, Level = LogLevel.Error,
        Message = "Failed to read installed.json file \"{InstalledJsonPath}\" in GOG Heroic library")]
    private partial void LogGOGHeroicFailedToReadJsonFile(string installedJsonPath, Exception ex);

    [LoggerMessage(EventId = 1704, Level = LogLevel.Error,
        Message = "Failed to scan the json block \"{Json}\" in GOG Heroic library")]
    private partial void LogGOGHeroicFailedToScanJsonBlock(string json, Exception ex);

    [LoggerMessage(EventId = 1705, Level = LogLevel.Debug,
        Message = "Failed to find the install path for GOG Heroic game with AppName: \"{AppName}\"")]
    private partial void LogGOGHeroicInstallPathNotFound(string? appName);

    [LoggerMessage(EventId = 1706, Level = LogLevel.Debug,
        Message = "Failed to find the name for GOG Heroic Game with \"{AppName}\" and install path: \"{InstallPath}\"")]
    private partial void LogGOGHeroicGameNameNotFound(string? appName, string installPath);

    [LoggerMessage(EventId = 1707, Level = LogLevel.Debug,
        Message = "Failed to find the app_name in \"{BlockValue}\"")]
    private partial void LogGOGHeroicAppNameNotFound(string blockValue);

    [LoggerMessage(EventId = 1708, Level = LogLevel.Error,
        Message = "Failed to read gog_install_info.json file \"{InstallInfoJson}\" in GOG Heroic library")]
    private partial void LogGOGHeroicFailedToReadInstallInfoFile(string installInfoJson, Exception ex);
}