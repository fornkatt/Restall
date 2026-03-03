using System;
using System.Threading.Tasks;

namespace Restall.Services;

public interface ILogService
{
    void LogInfo(string message, string logFileName = "log.txt");
    void LogWarning(string message, string logFileName = "log.txt");
    void LogError(string message, Exception? exception = null, string logFileName = "log.txt");
    
    Task LogInfoAsync(string message, string logFileName = "log.txt");
    Task LogWarningAsync(string message, string logFileName = "log.txt");
    Task LogErrorAsync(string message, Exception? exception = null, string logFileName = "log.txt");
}

public enum MessageType
{
    Info,
    Warning,
    Error
}