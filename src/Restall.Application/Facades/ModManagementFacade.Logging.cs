using Microsoft.Extensions.Logging;

namespace Restall.Application.Facades;

// Mod Management Facade Logging — EventId range: 1350 - 1399
public sealed partial class ModManagementFacade
{
    [LoggerMessage(EventId = 1350, Level = LogLevel.Error,
        Message = "{Message}")]
    private partial void LogError(string message, Exception exception);
}