using Restall.Application.Interfaces.Driven;
using System.Diagnostics;

namespace Restall.Infrastructure.Services;

internal sealed class FileExtractionService : IFileExtractionService
{
    private readonly ILogService _logService;
    private readonly IExtractionProcessRunner _processRunner;

    public FileExtractionService(ILogService logService)
        : this(logService, new DefaultExtractionProcessRunner())
    {
    }

    internal FileExtractionService(ILogService logService, IExtractionProcessRunner processRunner)
    {
        _logService = logService;
        _processRunner = processRunner;
    }

    public bool ExtractFiles(string fileToOpen, string[] filesToExtract, string destinationPath)
    {
        var toolPath = GetExtractionToolPath();

        if (toolPath == null)
        {
            _logService.LogInfo(
                OperatingSystem.IsLinux()
                ? "No extraction tool found. Ensure bsdtar (libarchive-tools) is installed."
                : "No extraction tool found. Ensure tar is available on your system.");
            return false;
        }

        var fileList = string.Join(" ", filesToExtract.Select(f => $"\"{f}\""));

        Directory.CreateDirectory(destinationPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = toolPath,
            Arguments = $"-xf \"{fileToOpen}\" -C \"{destinationPath}\" {fileList}",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };


        try
        {
            var result = _processRunner.Run(startInfo);
            if (result is null)
            {
                _logService.LogInfo("Unable to start extraction process.");
                return false;
            }

            if (result.ExitCode != 0)
            {
                _logService.LogError($"Extraction failed with exit code " +
                                               $"{result.ExitCode}: {result.StandardError}");
                return false;
            }

            _logService.LogInfo($"Successfully extracted ({fileList}) to {destinationPath} using {toolPath}");
            return true;
        }
        catch (Exception ex)
        {
            _logService.LogError("Failed to extract files", ex);
            return false;
        }
    }

    private string? GetExtractionToolPath()
    {
        if (OperatingSystem.IsWindows())
            return FindExtractionTool("where", "tar");

        if (OperatingSystem.IsLinux())
            return FindExtractionTool("which", "bsdtar");

        return null;
    }

    private string? FindExtractionTool(string finder, string tool)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = finder,
                Arguments = tool,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            var result = _processRunner.Run(processInfo);

            if (result == null) return null;

            var output = result.StandardOutput.Trim();

            if (result.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                return output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        }
        catch (Exception ex)
        {
            _logService.LogError($"Could not find {tool}", ex);
        }

        return null;
    }
}

internal interface IExtractionProcessRunner
{
    ExtractionProcessResult? Run(ProcessStartInfo startInfo);
}

internal sealed record ExtractionProcessResult(int ExitCode, string StandardOutput, string StandardError);

internal sealed class DefaultExtractionProcessRunner : IExtractionProcessRunner
{
    public ExtractionProcessResult? Run(ProcessStartInfo startInfo)
    {
        using var process = Process.Start(startInfo);
        if (process == null) return null;

        process.WaitForExit();

        var stdout = startInfo.RedirectStandardOutput
            ? process.StandardOutput.ReadToEnd()
            : string.Empty;
        var stderr = startInfo.RedirectStandardError
            ? process.StandardError.ReadToEnd()
            : string.Empty;

        return new ExtractionProcessResult(process.ExitCode, stdout, stderr);
    }
}
