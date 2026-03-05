using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Restall.Services;

public class LogService : ILogService
{
    private const string DefaultLogFileName = "restall_log.txt";
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private void Log(string message, MessageType messageType, Exception? exception = null,
        string logFileName = DefaultLogFileName)
    {
        string logFilePath = GetLogFilePath(logFileName);
        
        string logFormat = FormatLogMessage(message, messageType, exception);
        
        _semaphore.Wait();
        try
        {
            File.AppendAllText(logFilePath, logFormat);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to write to log: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private async Task LogAsync(string message, MessageType messageType, Exception? exception = null,
        string logFileName = DefaultLogFileName)
    {
        string logFilePath = GetLogFilePath(logFileName);
        
        string logFormat = FormatLogMessage(message, messageType, exception);
        
        await _semaphore.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(logFilePath, logFormat);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to write to log: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private static string FormatLogMessage(string message, MessageType messageType, Exception? exception)
    {
        return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {messageType} | {message}" +
               $"{(exception != null ? $" || {exception.Message}" : "")}{Environment.NewLine}";
    }

    private static string GetLogFilePath(string logFileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", logFileName);
    }
    
    public void LogInfo(string message, string logFileName = DefaultLogFileName) =>
        Log(message, MessageType.Info, null, logFileName);
    public void LogWarning(string message, string logFileName = DefaultLogFileName) =>
        Log(message, MessageType.Warning, null, logFileName);
    public void LogError(string message, Exception? exception = null, string logFileName = DefaultLogFileName) =>
        Log(message, MessageType.Error, exception, logFileName);
    public async Task LogInfoAsync(string message, string logFileName = DefaultLogFileName) =>
        await LogAsync(message, MessageType.Info, null, logFileName);
    public async Task LogWarningAsync(string message, string logFileName = DefaultLogFileName) =>
        await LogAsync(message, MessageType.Warning,  null, logFileName);
    public async Task LogErrorAsync(string message, Exception? exception = null, string logFileName = DefaultLogFileName) =>
        await LogAsync(message, MessageType.Error, exception, logFileName);
}