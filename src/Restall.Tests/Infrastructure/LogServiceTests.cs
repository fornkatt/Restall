using Moq;
using Restall.Application.Interfaces.Driven;
using Restall.Infrastructure.Services;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Infrastructure;

public sealed class LogServiceTests
{
    // Verifies that sync logging writes info, warning, error and exception text to the default log file.
    [Fact]
    public void SyncLogMethods_WriteExpectedEntriesToDefaultLogFile()
    {
        using var temp = new TempDirectory();
        var sut = CreateService(temp.GetPath("logs"));

        sut.LogInfo("Info message");
        sut.LogWarning("Warning message");
        sut.LogError("Error message", new InvalidOperationException("Boom"));

        var contents = File.ReadAllText(temp.GetPath("logs", "restall_log.txt"));
        Assert.Contains("| Info | Info message", contents);
        Assert.Contains("| Warning | Warning message", contents);
        Assert.Contains("| Error | Error message || Boom", contents);
    }

    // Verifies that async logging creates the log directory and writes to a custom file.
    [Fact]
    public async Task AsyncLogMethods_WriteExpectedEntriesToCustomLogFileAndCreateDirectory()
    {
        using var temp = new TempDirectory();
        var logDirectory = temp.GetPath("logs");
        var sut = CreateService(logDirectory);

        await sut.LogInfoAsync("Info message", "custom.txt");
        await sut.LogWarningAsync("Warning message", "custom.txt");
        await sut.LogErrorAsync("Error message", new InvalidOperationException("Boom"), "custom.txt");

        var contents = File.ReadAllText(Path.Combine(logDirectory, "custom.txt"));
        Assert.True(Directory.Exists(logDirectory));
        Assert.Contains("| Info | Info message", contents);
        Assert.Contains("| Warning | Warning message", contents);
        Assert.Contains("| Error | Error message || Boom", contents);
    }

    private static LogService CreateService(string logDirectory)
    {
        var pathService = new Mock<IPathService>(MockBehavior.Strict);
        pathService.Setup(x => x.GetDefaultLogPath()).Returns(logDirectory);
        return new LogService(pathService.Object);
    }
}
