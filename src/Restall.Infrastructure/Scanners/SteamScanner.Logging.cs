using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Scanners;

// Steam Scanner Logging - EventId range: 1750 - 1799
internal sealed partial class SteamScanner
{
    [LoggerMessage(EventId = 1750, Level = LogLevel.Error,
        Message = "Failed to parse the acf files \"{Acf}\" in the Steam library")]
    private partial void LogSteamAcfFilesParseFailure(string acf, Exception ex);

    [LoggerMessage(EventId = 1751, Level = LogLevel.Error,
        Message = "Failed to read Steam's 'libraryfolder.vdf' library through \"{VdfPath}\"")]
    private partial void LogSteamLibraryFolderVdfReadFailure(string vdfPath, Exception ex);

    [LoggerMessage(EventId = 1752, Level = LogLevel.Debug,
        Message = "Could not find 'steamapps' in Steam library folder: \"{LibraryFolder}\"")]
    private partial void LogSteamAppsFolderNotFound(string libraryFolder);

    [LoggerMessage(EventId = 1753, Level = LogLevel.Debug,
        Message = "Could not find the name of the Steam game with AppId: \"{AppId}\" in acf: \"{Acf}\" in appmanifest")]
    private partial void LogSteamGameNameNotFound(string appId, string acf);

    [LoggerMessage(EventId = 1754, Level = LogLevel.Debug,
        Message = "Could not find the install directory for Steam game \"{Name}\" with AppId: \"{AppId}\"")]
    private partial void LogSteamInstallDirectoryNotFound(string name, string appId);

    [LoggerMessage(EventId = 1755, Level = LogLevel.Debug,
        Message = "Could not find the root path \"{RootPath}\" for Steam game \"{Name}\" with AppId: \"{AppId}\"")]
    private partial void LogSteamRootPathNotFound(string rootPath, string name, string appId);
}