namespace Restall.Application.Interfaces.Driven;

public interface ILogService
{
    void LogInfo(string message, string? logFileName = null);
    void LogWarning(string message, string? logFileName = null);
    void LogError(string message, Exception? exception = null, string? logFileName = null);
    
    Task LogInfoAsync(string message, string? logFileName = null);
    Task LogWarningAsync(string message, string? logFileName = null);
    Task LogErrorAsync(string message, Exception? exception = null, string? logFileName = null);
}

public enum MessageType
{
    Info,
    Warning,
    Error
}