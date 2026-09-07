using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Scanners;

// Epic Scanner Logging - EventId range: 1650 - 1699
internal sealed partial class EpicScanner
{
    [LoggerMessage(EventId = 1650, Level = LogLevel.Error,
        Message = "Failed to scan items in Epic Games manifest \"{File}\"")]
    private partial void LogEpicManifestScanFailure(string file, Exception ex);

    [LoggerMessage(EventId = 1651, Level = LogLevel.Error,
        Message = "Failed to read 'legendary_install_info.json' file \"{InstallInfoJson}\" in Epic Heroic library")]
    private partial void LogEpicHeroicInstallInfoReadFailure(string installInfoJson, Exception ex);

    [LoggerMessage(EventId = 1652, Level = LogLevel.Error,
        Message = "Failed to scan the JSON block \"{Json}\" in Epic Heroic library")]
    private partial void LogEpicHeroicJsonBlockScanFailure(string json, Exception ex);

    [LoggerMessage(EventId = 1653, Level = LogLevel.Warning,
        Message = "Could not find the install path for Epic Heroic game with AppName: \"{AppName}\"")]
    private partial void LogEpicHeroicInstallPathNotFound(string? appName);

    [LoggerMessage(EventId = 1654, Level = LogLevel.Warning,
        Message =
            "Could not find the name for Epic Heroic game with \"{AppName}\" and install path: \"{InstallPath}\"")]
    private partial void LogEpicHeroicGameNameNotFound(string? appName, string installPath);

    [LoggerMessage(EventId = 1655, Level = LogLevel.Warning,
        Message = "Could not find the name of the Epic game \"{File}\" in \"{Item}\" in Epic Games Manifest")]
    private partial void LogEpicGameNameNotFound(string file, string item);

    [LoggerMessage(EventId = 1656, Level = LogLevel.Warning,
        Message = "Could not find root path for Epic game \"{Name}\" with item: \"{Item}\"")]
    private partial void LogEpicGameRootPathNotFound(string name, string item);

    [LoggerMessage(EventId = 1657, Level = LogLevel.Warning,
        Message = "Could not find the 'app_name' in \"{BlockValue}\"")]
    private partial void LogEpicHeroicAppNameNotFound(string blockValue);

    [LoggerMessage(EventId = 1658, Level = LogLevel.Error,
        Message = "Could not read 'installed.json' file in \"{InstalledJsonPath}\" in Epic Heroic library")]
    private partial void LogEpicHeroicInstalledJsonReadFailure(string installedJsonPath, Exception ex);
}