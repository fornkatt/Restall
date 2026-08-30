using Microsoft.Extensions.Logging;

namespace Restall.Application.Logging;

// PE File Failures Logging — EventId range: 50 - 74
public static partial class Log
{
    [LoggerMessage(EventId = 50, Level = LogLevel.Warning,
        Message = "Failed to scan file {Filename}")]
    public static partial void PeFileReadFailure(this ILogger logger, string filename, Exception ex);
}