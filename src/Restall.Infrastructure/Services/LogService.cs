using Restall.Application.Interfaces.Driven;
using System.Diagnostics;

namespace Restall.Infrastructure.Services;

internal sealed class LogService : ILogService
{
    private const int s_maxLogFiles = 10;
    
    private readonly string _logsDirectory;
    private readonly string _defaultLogFilename;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public LogService(IPathService pathService)
    {
        _logsDirectory = pathService.GetDefaultLogPath();
        _defaultLogFilename = $"{DateTime.Now:yyyy-MM-dd}_restall_log.txt";
    }
    
    private string ResolveFilename(string? logFilename) =>
        logFilename ?? _defaultLogFilename;

    private void Log(string message, MessageType messageType, Exception? exception = null,
        string? logFilename = null)
    {
        var logFilePath = Path.Combine(_logsDirectory, ResolveFilename(logFilename));
        
        var logFormat = FormatLogMessage(message, messageType, exception);
        
        _semaphore.Wait();
        try
        {
            Directory.CreateDirectory(_logsDirectory);

            File.AppendAllText(logFilePath, logFormat);
            
            EnforceLogFileLimit();
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
        string? logFilename = null)
    {
        var logFilePath = Path.Combine(_logsDirectory, ResolveFilename(logFilename));
        
        var logFormat = FormatLogMessage(message, messageType, exception);
        
        await _semaphore.WaitAsync();
        try
        {
            Directory.CreateDirectory(_logsDirectory);

            await File.AppendAllTextAsync(logFilePath, logFormat);
            
            EnforceLogFileLimit();
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
        return $"{DateTime.Now:HH:mm:ss} | {messageType} | {message}" +
               $"{(exception != null ? $" || {exception.Message}" : "")}{Environment.NewLine}";
    }

    private void EnforceLogFileLimit()
    {
        var logFiles = Directory.GetFiles(_logsDirectory, "*_restall_log.txt")
            .OrderBy(File.GetCreationTimeUtc)
            .ToList();

        while (logFiles.Count > s_maxLogFiles)
        {
            File.Delete(logFiles.First());
            logFiles.RemoveAt(0);
        }
    }
    
    public void LogInfo(string message, string? logFilename = null) =>
        Log(message, MessageType.Info, null, logFilename);
    public void LogWarning(string message, string? logFilename = null) =>
        Log(message, MessageType.Warning, null, logFilename);
    public void LogError(string message, Exception? exception = null, string? logFilename = null) =>
        Log(message, MessageType.Error, exception, logFilename);
    public async Task LogInfoAsync(string message, string? logFilename = null) =>
        await LogAsync(message, MessageType.Info, null, logFilename);
    public async Task LogWarningAsync(string message, string? logFilename = null) =>
        await LogAsync(message, MessageType.Warning,  null, logFilename);
    public async Task LogErrorAsync(string message, Exception? exception = null, string? logFilename = null) =>
        await LogAsync(message, MessageType.Error, exception, logFilename);
}