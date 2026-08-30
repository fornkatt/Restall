using Microsoft.Extensions.Logging;

namespace Restall.Application.UseCases;

// RenoDX Install Logging — EventId range: 1300 - 1349
public sealed partial class InstallRenoDXUseCase
{
    [LoggerMessage(EventId = 1300, Level = LogLevel.Warning,
        Message = "Could not resolve RenoDX addon filename for {GameName} (Engine: {Engine}, Arch: {Arch})")]
    private partial void LogRenoDXAddonFilenameResolutionFailure(string gameName, string engine, string arch);

    [LoggerMessage(EventId = 1301, Level = LogLevel.Warning,
        Message =
            "Failed to read version from RenoDX file {Filename} for {GameName} — Service returned: {ErrorMessage}")]
    private partial void LogRenoDXVersionReadFailure(string filename, string gameName, string? errorMessage,
        Exception? ex);
    
    [LoggerMessage(EventId = 1302, Level = LogLevel.Warning,
        Message = "Failed to delete stale RenoDX cache file at {CachedFile} — Service returned: {ErrorMessage}")]
    private partial void LogRenoDXStaleCacheDeletionFailure(string cachedFile, string? errorMessage, Exception? ex);
}