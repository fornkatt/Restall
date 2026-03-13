using System.Diagnostics;
using Restall.Application.Interfaces;

namespace Restall.Infrastructure.Services;

public class FileExtractionService : IFileExtractionService
{
    private readonly ILogService _logService;

    public FileExtractionService(ILogService logService)
    {
        _logService = logService;
    }

    public bool ExtractFiles(string fileToOpen, string[] filesToExtract, string destinationPath)
    {
        var sevenZipPath = GetSevenZipPath();

        if (sevenZipPath == null)
        {
            _logService.LogInfo($"7-Zip executable not found. Install it to" +
                                          $" {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "7z")}" +
                                          $" or reinstall the program.");
            return false;
        }

        var fileList = string.Join(" ", filesToExtract.Select(f => $"\"{f}\""));

        var startInfo = new ProcessStartInfo
        {
            FileName = sevenZipPath,
            Arguments = $"e \"{fileToOpen}\" -o\"{destinationPath}\" {fileList} -y",
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
                _logService.LogInfo("Unable to start 7-Zip process or unsupported operating system.");
                return false;
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var stderr = process.StandardError.ReadToEnd();
                _logService.LogError($"7-Zip extraction failed with exit code " +
                                               $"{process.ExitCode}: {stderr}");
                return false;
            }

            _logService.LogInfo($"Successfully extracted ({fileList}) to {destinationPath}");
            return true;
        }
        catch (Exception ex)
        {
            _logService.LogError("Failed to extract files", ex);
            return false;
        }
    }

    private string? GetSevenZipPath()
    {
        string windowsSevenZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "7z", "7za.exe");
        string linuxSevenZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "7z", "7zzs");
        
        if (OperatingSystem.IsWindows())
        {
            return !File.Exists(windowsSevenZipPath) ? null : Path.Combine(windowsSevenZipPath);
        }

        if (OperatingSystem.IsLinux())
        {
            var systemPath = FindSystemSevenZip();
            if (systemPath != null)
            {
                return systemPath;
            }
            
            if (!File.Exists(linuxSevenZipPath))
            {
                return null;
            }
            
            EnsureExecutable(linuxSevenZipPath);
            return linuxSevenZipPath;
        }

        return null;
    }

    /* Linux methods */
    private string? FindSystemSevenZip()
    {
        string[] candidates = ["7zz", "7z", "7za"];

        foreach (var candidate in candidates)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = candidate,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                });

                if (process == null) continue;

                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    return candidate;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError("Could not find suitable system 7-Zip candidate.", ex);
            }
        }

        return null;
    }

    private void EnsureExecutable(string path)
    {
        if (!File.Exists(path))
        {
            _logService.LogInfo($"File {path} not found.");
            return;
        }

        if (IsExecutable(path))
        {
            _logService.LogInfo($"File {path} is already executable.");
            return;
        }

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{path}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            process?.WaitForExit();
        }
        catch (Exception ex)
        {
            _logService.LogError($"Could not find executable at {path}", ex);
        }
    }

    private bool IsExecutable(string path)
    {
        try
        {
            var unixFileMode = File.GetUnixFileMode(path);
            return (unixFileMode & UnixFileMode.UserExecute) != 0;
        }
        catch
        {
            _logService.LogError($"Failed to asses executable status of {path}, please ensure the file exists.");
            return false;
        }
    }
}
