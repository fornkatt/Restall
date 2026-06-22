using Restall.Application.Interfaces.Driven;

namespace Restall.Tests.TestUtilities;

internal sealed class NoOpLogService : ILogService
{
    public void LogInfo(string message, string logFileName = "restall_log.txt")
    {
    }

    public void LogWarning(string message, string logFileName = "restall_log.txt")
    {
    }

    public void LogError(string message, Exception? exception = null, string logFileName = "restall_log.txt")
    {
    }

    public Task LogInfoAsync(string message, string logFileName = "restall_log.txt") => Task.CompletedTask;

    public Task LogWarningAsync(string message, string logFileName = "restall_log.txt") => Task.CompletedTask;

    public Task LogErrorAsync(string message, Exception? exception = null, string logFileName = "restall_log.txt") =>
        Task.CompletedTask;
}
