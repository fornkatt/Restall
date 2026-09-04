using Microsoft.Extensions.Logging;

namespace Restall.Application.Logging;

// PE File Failures Logging — EventId range: 50 - 74
public static partial class Log
{
    [LoggerMessage(EventId = 50, Level = LogLevel.Warning,
        Message = "Failed to scan file {Filename}")]
    public static partial void PeFileReadFailure(this ILogger logger, string filename, Exception ex);
    
    // TODO: once Result type is available to this method, write out a reason or failure message
    [LoggerMessage(EventId = 51, Level = LogLevel.Warning,
        Message= "Failed to get icon from executable file from [{ExePath}]")]
    public static partial void PeFileIconScanFailure(this ILogger logger, string exePath);
}