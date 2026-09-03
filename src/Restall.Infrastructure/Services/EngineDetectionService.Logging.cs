using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Services;
// Engine Detection Logging - EventId range: 1450-1499
internal sealed partial class EngineDetectionService
{
    [LoggerMessage(EventId = 1450, Level = LogLevel.Error,
        Message = "Couldn't collect the binaries from [{Dir}] folder")]
    private partial void LogFailedToCollectUEBinaries(string dir, Exception ex);

    [LoggerMessage(EventId = 1451, Level = LogLevel.Error,
        Message = "Failed to find shallow files in [{Folder}] folder")]
    private partial void LogFailedToFindShallowFiles(string folder, Exception ex);

    [LoggerMessage(EventId = 1452, Level = LogLevel.Error,
        Message = "Failed to find shallow exe folder in [{Root}] folder ")]
    private partial void LogFailedToFindShallowExeFolder(string root, Exception ex);
    
    [LoggerMessage(EventId = 1453, Level = LogLevel.Debug,
    Message = "UE Binaries scan hit max depth [5] at [{Dir}] ")]
    private partial void LogUEBinariesScanHitMaxDepth(string dir);
    
    [LoggerMessage(EventId = 1454, Level = LogLevel.Debug,
        Message = "No executable folder found under [{Root}]")]
    private partial void LogNoExecutableFolderFound(string root);
    
    [LoggerMessage(EventId = 1455, Level = LogLevel.Debug,
    Message = "Found Executable via BFS at depth: [{Depth}] in [{Dir}]")]
    private partial void LogFoundExecutableViaBFS(int depth, string dir);
}