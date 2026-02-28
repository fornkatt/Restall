using System;
using System.Threading.Tasks;

namespace Restall.Services;

public interface ILogService
{
    Task LogAsync(string message, MessageType messageType, Exception? exception = null);

    Task LogInfoAsync(string message) => LogAsync(message, MessageType.Info);
    Task LogWarningAsync(string message) => LogAsync(message, MessageType.Warning);
    Task LogErrorAsync(string message, Exception? exception = null) => LogAsync(message, MessageType.Error, exception);
}

public enum MessageType
{
    Info,
    Warning,
    Error
}