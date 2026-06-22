using System.Diagnostics;
using Moq;
using Restall.Application.Interfaces.Driven;
using Restall.Infrastructure.Services;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Infrastructure;

public sealed class FileExtractionServiceTests
{
    // Verifies that a missing extraction tool returns false and logs an informational message.
    [Fact]
    public void ExtractFiles_WhenExtractionToolIsMissing_ReturnsFalseAndLogsInfo()
    {
        using var temp = new TempDirectory();
        var log = new Mock<ILogService>(MockBehavior.Loose);
        var runner = new FakeExtractionProcessRunner();
        runner.Enqueue(_ => new ExtractionProcessResult(1, string.Empty, string.Empty));
        var sut = new FileExtractionService(log.Object, runner);

        var result = sut.ExtractFiles(temp.GetPath("installer.exe"), ["ReShade64.dll"], temp.GetPath("extract"));

        Assert.False(result);
        log.Verify(x => x.LogInfo(
            It.Is<string>(message => message.Contains("No extraction tool found")),
            It.IsAny<string>()), Times.Once);
    }

    // Verifies that process start failures return false after the extraction tool was found.
    [Fact]
    public void ExtractFiles_WhenExtractionProcessCannotStart_ReturnsFalseAndLogsInfo()
    {
        using var temp = new TempDirectory();
        var log = new Mock<ILogService>(MockBehavior.Loose);
        var runner = new FakeExtractionProcessRunner();
        runner.Enqueue(_ => new ExtractionProcessResult(0, "tar.exe", string.Empty));
        runner.Enqueue(_ => null);
        var sut = new FileExtractionService(log.Object, runner);

        var result = sut.ExtractFiles(temp.GetPath("installer.exe"), ["ReShade64.dll"], temp.GetPath("extract"));

        Assert.False(result);
        log.Verify(x => x.LogInfo("Unable to start extraction process.", It.IsAny<string>()), Times.Once);
    }

    // Verifies that non-zero extraction exit codes return false and log stderr.
    [Fact]
    public void ExtractFiles_WhenExtractionProcessFails_ReturnsFalseAndLogsError()
    {
        using var temp = new TempDirectory();
        var log = new Mock<ILogService>(MockBehavior.Loose);
        var runner = new FakeExtractionProcessRunner();
        runner.Enqueue(_ => new ExtractionProcessResult(0, "tar.exe", string.Empty));
        runner.Enqueue(_ => new ExtractionProcessResult(2, string.Empty, "bad archive"));
        var sut = new FileExtractionService(log.Object, runner);

        var result = sut.ExtractFiles(temp.GetPath("installer.exe"), ["ReShade64.dll"], temp.GetPath("extract"));

        Assert.False(result);
        log.Verify(x => x.LogError(
            It.Is<string>(message => message.Contains("Extraction failed with exit code 2") && message.Contains("bad archive")),
            It.IsAny<Exception?>(),
            It.IsAny<string>()), Times.Once);
    }

    // Verifies that successful extraction returns true and creates the destination directory.
    [Fact]
    public void ExtractFiles_WhenExtractionProcessSucceeds_ReturnsTrueAndCreatesDestination()
    {
        using var temp = new TempDirectory();
        var destination = temp.GetPath("extract");
        var archive = temp.GetPath("installer.exe");
        var log = new Mock<ILogService>(MockBehavior.Loose);
        var runner = new FakeExtractionProcessRunner();
        runner.Enqueue(_ => new ExtractionProcessResult(0, "C:\\Tools\\tar.exe", string.Empty));
        runner.Enqueue(_ => new ExtractionProcessResult(0, string.Empty, string.Empty));
        var sut = new FileExtractionService(log.Object, runner);

        var result = sut.ExtractFiles(archive, ["ReShade64.dll", "ReShade32.dll"], destination);

        Assert.True(result);
        Assert.True(Directory.Exists(destination));
        Assert.Equal(2, runner.Calls.Count);
        Assert.Equal("C:\\Tools\\tar.exe", runner.Calls[1].FileName);
        Assert.Contains($"-xf \"{archive}\"", runner.Calls[1].Arguments);
        Assert.Contains($"-C \"{destination}\"", runner.Calls[1].Arguments);
        Assert.Contains("\"ReShade64.dll\"", runner.Calls[1].Arguments);
        Assert.Contains("\"ReShade32.dll\"", runner.Calls[1].Arguments);
    }

    private sealed class FakeExtractionProcessRunner : IExtractionProcessRunner
    {
        private readonly Queue<Func<ProcessStartInfo, ExtractionProcessResult?>> _responses = [];

        public List<ProcessStartInfo> Calls { get; } = [];

        public void Enqueue(Func<ProcessStartInfo, ExtractionProcessResult?> response) => _responses.Enqueue(response);

        public ExtractionProcessResult? Run(ProcessStartInfo startInfo)
        {
            Calls.Add(startInfo);
            return _responses.Dequeue()(startInfo);
        }
    }
}
