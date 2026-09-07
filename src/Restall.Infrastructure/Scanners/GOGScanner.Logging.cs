using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Restall.Infrastructure.Scanners;

// GOG Scanner Logging - EventId range: 1700 - 1749
internal sealed partial class GOGScanner
{
    [LoggerMessage(EventId = 1700, Level = LogLevel.Error,
        Message = "Failed to scan the GOG Galaxy library \"{Error}\"")]
    private partial void LogGOGLibraryScanFailure(string error, Exception ex);

    [LoggerMessage(EventId = 1701, Level = LogLevel.Debug,
        Message = "Could not find the directory for GOG game \"{Name}\" in \"{SubKey}\"")]
    private partial void LogGOGInstallPathNotFound(string name, string subKey);

    [LoggerMessage(EventId = 1702, Level = LogLevel.Debug,
        Message = "Display name for \"{SubName}\" in GOG Scanner is empty")]
    private partial void LogGOGGameDisplayNameEmpty(string subName);

    [LoggerMessage(EventId = 1703, Level = LogLevel.Error,
        Message = "Failed to read 'installed.json' file \"{InstalledJsonPath}\" in GOG Heroic library")]
    private partial void LogGOGHeroicJsonFileReadFailure(string installedJsonPath, Exception ex);

    [LoggerMessage(EventId = 1704, Level = LogLevel.Error,
        Message = "Failed to scan the json block \"{Json}\" in GOG Heroic library")]
    private partial void LogGOGHeroicJsonBlockScanFailure(string json, Exception ex);

    [LoggerMessage(EventId = 1705, Level = LogLevel.Debug,
        Message = "Could not find the install path for GOG Heroic game with appName: \"{AppName}\"")]
    private partial void LogGOGHeroicInstallPathNotFound(string? appName);

    [LoggerMessage(EventId = 1706, Level = LogLevel.Debug,
        Message = "Could not find the name for GOG Heroic game with \"{AppName}\" and install path: \"{InstallPath}\"")]
    private partial void LogGOGHeroicGameNameNotFound(string? appName, string installPath);

    [LoggerMessage(EventId = 1707, Level = LogLevel.Debug,
        Message = "Could not find the 'appName' for GOG Heroic game at \"{installPath}\"")]
    private partial void LogGOGHeroicAppNameNotFound(string? installPath);

    [LoggerMessage(EventId = 1708, Level = LogLevel.Error,
        Message = "Failed to read 'gog_install_info.json' file \"{InstallInfoJson}\" in GOG Heroic library")]
    private partial void LogGOGHeroicInstallInfoReadFailure(string installInfoJson, Exception ex);

    [LoggerMessage(EventId = 1709, Level = LogLevel.Warning,
        Message = "No entries found in GOG Heroic install info file \"{InstallInfoPath}\"" +
                  " — all GOG Heroic games will be skipped")]
    private partial void LogGOGHeroicInstallInfoEmpty(string installInfoPath);

    [LoggerMessage(EventId = 1710, Level = LogLevel.Debug,
        Message = "Could not find install info entry for GOG Heroic game \"{AppName}\" in \"{InstallInfoPath}\"")]
    private partial void LogGOGHeroicInstallInfoEntryNotFound(string? appName, string installInfoPath);
}