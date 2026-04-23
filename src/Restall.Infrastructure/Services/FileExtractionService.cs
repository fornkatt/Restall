using System.ComponentModel;
using Restall.Application.Interfaces.Driven;
using System.Diagnostics;
using Restall.Application.Common;

namespace Restall.Infrastructure.Services;

internal sealed class FileExtractionService : IFileExtractionService
{
    public Result ExtractFiles(string fileToOpen, string[] filesToExtract, string destinationPath)
    {
        var toolPath = GetExtractionToolPath();

        if (toolPath == null)
        {
            return Result.Err(OperatingSystem.IsLinux()
                ? "No extraction tool found. Ensure bsdtar (libarchive-tools) is installed."
                : "No extraction tool found. Ensure tar is available on your system.", ResultError.ToolNotFound);
        }

        var fileList = string.Join(" ", filesToExtract.Select(f => $"\"{f}\""));

        try
        {
            Directory.CreateDirectory(destinationPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Err("Permission denied creating destination directory", ResultError.PermissionDenied, ex);
        }
        catch (IOException ex)
        {
            return Result.Err("Failed to create destination directory", ResultError.FileSystemError, ex);
        }

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
                return Result.Err("Unable to start extraction process.", ResultError.ProcessStartFailed);
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var stderr = process.StandardError.ReadToEnd();
                return Result.Err($"""
                                   Extraction failed with exit code
                                   {process.ExitCode}: {stderr}
                                   """, ResultError.ExtractionFailed);
            }
        }
        catch (Win32Exception ex)
        {
            return Result.Err("Failed to start extraction process", ResultError.ProcessStartFailed, ex);
        }

        return Result.Ok();
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
        catch (Win32Exception)
        {
            return null;
        }

        return null;
    }
}