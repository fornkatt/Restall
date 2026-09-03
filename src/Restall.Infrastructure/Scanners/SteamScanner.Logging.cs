using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Scanners;

// Steam Scanner Logging - EventId range: 1750 - 1799
internal sealed partial class SteamScanner
{
    [LoggerMessage(EventId = 1750, Level = LogLevel.Error,
        Message="Failed to parse acf files [{Acf}] in Steam library")]
    private partial void LogSteamFailedToParseAcfFiles(string acf, Exception ex);
    
    [LoggerMessage(EventId = 1751, Level = LogLevel.Error,
        Message="Failed to read Steam's libraryfolder.vdf library through [{VdfPath}]")]
    private partial void LogSteamFailedToReadLibraryFoldersVdfLibrary(string vdfPath, Exception ex);
    
    [LoggerMessage(EventId= 1752, Level = LogLevel.Debug,
        Message="Can not find steamapps in LibraryFolder: {LibraryFolder}")]
    private partial void LogSteamAppsNotFoundInLibraryFolder(string libraryFolder);
    
    [LoggerMessage(EventId = 1753, Level = LogLevel.Debug,
        Message = "Can not find the name of the Steam Game [{AppId}] in [{Acf}] in appmanifest")]
    private partial void LogSteamGameNameNotFound(string appId,string acf);
    [LoggerMessage(EventId = 1754, Level = LogLevel.Debug,
        Message = "Can not find Install directory for Steam Game [{Name}] with AppId: [{AppId}]")]
    private partial void LogSteamInstallDirectoryNotFound(string name,string appId);
    
    [LoggerMessage(EventId = 1755, Level = LogLevel.Debug,
        Message = "Can not find the root path [{rootPath}] for Steam Game [{Name}] with AppId: [{AppId}] ")]
    private partial void LogSteamRootPathNotFound(string rootPath, string name,string appId);
    
    
}