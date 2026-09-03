using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Scanners;

// Epic Scanner Logging - EventId range: 1650 - 1699
internal sealed partial class EpicScanner
{
  [LoggerMessage(EventId = 1650, Level = LogLevel.Error,
    Message = "Failed to scan items in Epic Games manifest {File}")]
  private partial void LogFailedToScanManifest(string File, Exception ex);
  
  [LoggerMessage(EventId=1651, Level = LogLevel.Error,
    Message="Failed to read installed.json file [{installedJsonPath}] in Epic Heroic library")]
  private partial void LogEpicHeroicFailedToReadJsonFile(string installedJsonPath, Exception ex);
  
  [LoggerMessage(EventId=1652, Level = LogLevel.Error,
    Message="Failed to scan the json block [{json}] in Epic Heroic library")]
  private partial void LogEpicHeroicFailedToScanJsonBlock(string json, Exception ex);
  
  [LoggerMessage(EventId=1653, Level = LogLevel.Debug,
    Message="Failed to find install path for Epic Heroic game with AppName: [{appName}] ")]
  private partial void LogEpicHeroicFailedToFindInstallPath(string? appName);
  [LoggerMessage(EventId=1654, Level = LogLevel.Debug,
    Message = "Failed to find name for Epic Heroic Game with [{AppName}] with install path: [{InstallPath}]")]
  private partial void LogEpicHeroicFailedToFindName(string? appName, string installPath);

  [LoggerMessage(EventId = 1655, Level = LogLevel.Debug,
    Message = "Can not find the name of the Epic Game [{File}] in [{Item}] in Epic Manifest")]
  private partial void LogEpicGameNameNotFound(string file, string item);
  [LoggerMessage(EventId = 1656, Level = LogLevel.Debug,
    Message = "Can not find Root Path for Epic Game [{Name}] with Item: [{Item}]")]
  private partial void LogEpicGameRootPathNotFound(string name, string item);
  

}