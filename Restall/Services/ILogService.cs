using System;
using System.Threading.Tasks;

namespace Restall.Services;

public interface ILogService
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
    
    Task LogInfoAsync(string message);
    Task LogWarningAsync(string message);
    Task LogErrorAsync(string message, Exception? exception = null);
}

public enum MessageType
{
    Info,
    Warning,
    Error
}