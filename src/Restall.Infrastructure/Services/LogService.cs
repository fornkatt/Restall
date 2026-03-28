using Restall.Application.Interfaces.Driven;
using System.Diagnostics;

namespace Restall.Infrastructure.Services;

internal sealed class LogService : ILogService
{
    private readonly string _logsDirectory;

    private const string s_defaultLogFilename = "restall_log.txt";

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public LogService(IPathService pathService)
    {
        _logsDirectory = pathService.GetDefaultLogPath();
    }

    private void Log(string message, MessageType messageType, Exception? exception = null,
        string logFilename = s_defaultLogFilename)
    {
        string logFilePath = Path.Combine(_logsDirectory, logFilename);
        
        string logFormat = FormatLogMessage(message, messageType, exception);
        
        _semaphore.Wait();
        try
        {
            Directory.CreateDirectory(_logsDirectory);

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
        string logFilename = s_defaultLogFilename)
    {
        string logFilePath = Path.Combine(_logsDirectory, logFilename);
        
        string logFormat = FormatLogMessage(message, messageType, exception);
        
        await _semaphore.WaitAsync();
        try
        {
            Directory.CreateDirectory(_logsDirectory);

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
    
    public void LogInfo(string message, string logFilename = s_defaultLogFilename) =>
        Log(message, MessageType.Info, null, logFilename);
    public void LogWarning(string message, string logFilename = s_defaultLogFilename) =>
        Log(message, MessageType.Warning, null, logFilename);
    public void LogError(string message, Exception? exception = null, string logFilename = s_defaultLogFilename) =>
        Log(message, MessageType.Error, exception, logFilename);
    public async Task LogInfoAsync(string message, string logFilename = s_defaultLogFilename) =>
        await LogAsync(message, MessageType.Info, null, logFilename);
    public async Task LogWarningAsync(string message, string logFilename = s_defaultLogFilename) =>
        await LogAsync(message, MessageType.Warning,  null, logFilename);
    public async Task LogErrorAsync(string message, Exception? exception = null, string logFilename = s_defaultLogFilename) =>
        await LogAsync(message, MessageType.Error, exception, logFilename);
}