using Restall.Application.Interfaces.Driven;
using System.Diagnostics;

namespace Restall.Infrastructure.Services;

internal sealed class FileExtractionService : IFileExtractionService
{
    private readonly ILogService _logService;

    public FileExtractionService(ILogService logService)
    {
        _logService = logService;
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
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logService.LogInfo("Unable to start extraction process.");
                return false;
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var stderr = process.StandardError.ReadToEnd();
                _logService.LogError($"Extraction failed with exit code " +
                                               $"{process.ExitCode}: {stderr}");
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
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = finder,
                Arguments = tool,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            });

            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                return output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        }
        catch (Exception ex)
        {
            _logService.LogError($"Could not find {tool}", ex);
        }

        return null;
    }
}
