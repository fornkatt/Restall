using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Restall.Services;

public class LogService : ILogService
{
    private readonly string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log.txt");
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task LogAsync(string message, MessageType messageType, Exception? exception = null)
    {
        string logFormat = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {messageType} | {message}" +
                           $"{(exception != null ? $" || {exception.Message}" : "")}{Environment.NewLine}";

        await _semaphore.WaitAsync();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath)!);
            await File.AppendAllTextAsync(_logFilePath, logFormat);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}