using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Services;

// Game Cover Logging — EventId range: 1400 - 1449
internal sealed partial class GameCoverService
{
    [LoggerMessage(EventId = 1400, Level = LogLevel.Debug,
        Message = "Could not find  Game Cover for [{GameName}] from [{CoverPath}]")]
    private partial void LogGameCoverRetrievalMissing(string gameName, string coverPath);

    [LoggerMessage(EventId = 1401, Level = LogLevel.Debug,
        Message = "Downloading Game cover of [{GameName}] from [{Source}]")]
    private partial void LogGameCoverDownload(string gameName, string source);

    [LoggerMessage(EventId = 1402, Level = LogLevel.Debug,
        Message = "Copying Game cover of [{GameName}] from [{Source}]")]
    private partial void LogGameCoverCopy(string gameName, string source);
    
    [LoggerMessage(EventId = 1403, Level = LogLevel.Error,
        Message = "Failed to copy Game cover for [{GameName}]")]
    private partial void LogGameCoverCopyFailed(string gameName, Exception ex);

    [LoggerMessage(EventId = 1404, Level = LogLevel.Debug,
        Message = "Could not the find steam root for [{GameName}]")]
    private partial void LogSteamCoverCopyFailed(string gameName);
    
    [LoggerMessage(EventId=1405, Level = LogLevel.Debug,
        Message="Found [{GameName}] in guid: [{GuidDir}], Product ID: [{ProductId}]")]
    private partial void LogGOGLocalGameFound(string gameName, string guidDir, string productId);
    
    [LoggerMessage(EventId=1406, Level = LogLevel.Error,
        Message="Failed to scan local GOG Game cover for [{GameName}] in guid: [{GuidDir}], Product ID: [{ProductId}]")]
    private partial void LogGOGLocalGameCoverScanFailed(string gameName, string guidDir, string productId, Exception ex);
    
    [LoggerMessage(EventId=1407, Level = LogLevel.Error,
        Message="GOG API Game cover lookup failed [{GameName}]")]
    private partial void LogGOGApiCoverLookupFailed(string gameName, Exception ex);
    
    [LoggerMessage(EventId=1408, Level=LogLevel.Debug,
        Message="Heroic Cache file was not found at [{GameName}] at the cached file: [{CacheFile}]")]
    private partial void LogHeroicCacheFileNotFound(string gameName, string cacheFile);

    [LoggerMessage(EventId = 1409, Level = LogLevel.Debug,
        Message = "The Heroic game entry for [{GameName}] with ID: [{GameId}] was not found at the cached file: [{CacheFile}]")]
    private partial void LogHeroicGameNotFound(string gameName, string gameId, string cacheFile);
    
    [LoggerMessage(EventId = 1410, Level = LogLevel.Error,
        Message = "Failed to look up the Heroic game entry for [{GameName}] in the cached file: [{CacheFile}]")]
    private partial void LogHeroicCacheFileLookupFailed(string gameName, string cacheFile, Exception ex);

    [LoggerMessage(EventId = 1411, Level = LogLevel.Debug,
        Message = "Failed to do 'exact' look up for [{GameName}] with URL: [{ExactUrl}]. Proceeding to API Cargo" )]
    private partial void LogPCGamingWikiExactUrlLookupFailed(string gameName, string exactUrl);
    [LoggerMessage(EventId=1412, Level = LogLevel.Error,
        Message="Failed to search for [{GameName}]'s cover at PC Gaming Wiki")]
    private partial void LogPCGamingWikiSearchFailed(string gameName, Exception ex);
    
    [LoggerMessage(EventId=14013, Level = LogLevel.Error,
        Message="Failed to retrieve the API Cargo from the URL at PC Gaming Wiki: [{ApiUrl}]")]
    private partial void LogPCGamingWikiCargoApiFailed(string apiUrl, Exception ex);
    
    [LoggerMessage(EventId=1414, Level = LogLevel.Debug,
        Message="Failed to retrieve the [{GameName}]'s Page Id: [{PageId}]. Proceeding to TopSearchPageId. ")]
    private partial void LogPCGamingWikiPageNotFound(string gameName, string pageId);
    
    [LoggerMessage(EventId = 1415, Level = LogLevel.Debug,
        Message= "The download cover for [{GameName}] was successful from PC Gaming Wiki. Cover path: [{CoverPath}] | CoverUrl: [{CoverUrl}]")]
    private partial void LogDownloadCoverSuccessful(string gameName, string coverPath, string coverUrl);

    [LoggerMessage(EventId = 1416, Level = LogLevel.Error,
        Message =
            "Failed to download the cover for [{GameName}] from PC Gaming Wiki. Cover path: [{CoverPath}] | CoverUrl: [{CoverUrl}]")]
    private partial void LogDownloadCoverFailed(string gameName, string coverPath, string coverUrl, Exception ex);

}