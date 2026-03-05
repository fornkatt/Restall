namespace Restall.Application.Interfaces;

public interface ILogService
{
    private const string DefaultLogFileName = "restall_log.txt";
    
    void LogInfo(string message, string logFileName = DefaultLogFileName);
    void LogWarning(string message, string logFileName = DefaultLogFileName);
    void LogError(string message, Exception? exception = null, string logFileName = DefaultLogFileName);
    
    Task LogInfoAsync(string message, string logFileName = DefaultLogFileName);
    Task LogWarningAsync(string message, string logFileName = DefaultLogFileName);
    Task LogErrorAsync(string message, Exception? exception = null, string logFileName = DefaultLogFileName);
}

public enum MessageType
{
    Info,
    Warning,
    Error
}