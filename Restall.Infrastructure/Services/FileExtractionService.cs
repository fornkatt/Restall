using Restall.Application.Interfaces;
using System.Diagnostics;

namespace Restall.Infrastructure.Services;

internal sealed class FileExtractionService : IFileExtractionService
{
    private readonly ILogService _logService;

    private readonly string _tools7zBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "7z");

    public FileExtractionService(ILogService logService)
    {
        _logService = logService;
    }

    public bool ExtractFiles(string fileToOpen, string[] filesToExtract, string destinationPath)
    {
        var toolPath = GetExtractionToolPath();

        if (toolPath == null)
        {
            _logService.LogInfo($"No extraction tool found. Ensure Windows tar or 7zip is installed if on Linux. Alternatively install 7zip to" +
                                          $" {Path.Combine(_tools7zBasePath, "7z")}" +
                                          $" or reinstall the program to restore bundled executables.");
            return false;
        }

        var fileList = string.Join(" ", filesToExtract.Select(f => $"\"{f}\""));

        var isTar = Path.GetFileNameWithoutExtension(toolPath)
            .Equals("tar", StringComparison.OrdinalIgnoreCase);

        Directory.CreateDirectory(destinationPath);

        var arguments = isTar
            ? $"-xf \"{fileToOpen}\" -C \"{destinationPath}\" {fileList}"
            : $"e \"{fileToOpen}\" -o\"{destinationPath}\" {fileList} -y";

        var startInfo = new ProcessStartInfo
        {
            FileName = toolPath,
            Arguments = arguments,
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
                _logService.LogInfo("Unable to start extraction process or unsupported operating system.");
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
        string windowsSevenZipPath = Path.Combine(_tools7zBasePath, "7za.exe");
        string linuxSevenZipPath = Path.Combine(_tools7zBasePath, "7zzs");
        
        if (OperatingSystem.IsWindows())
        {
            var tarPath = FindWindowsTar();
            if (tarPath is not null) return tarPath;

            return !File.Exists(windowsSevenZipPath) ? null : windowsSevenZipPath;
        }

        if (OperatingSystem.IsLinux())
        {
            var systemPath = FindSystemSevenZip();
            if (systemPath is not null) return systemPath;
            
            if (!File.Exists(linuxSevenZipPath)) return null;
            
            EnsureExecutable(linuxSevenZipPath);
            return linuxSevenZipPath;
        }

        return null;
    }

    /* Windows methods */
    private string? FindWindowsTar()
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "where",
                Arguments = "tar",
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
            _logService.LogError("Could not find system tar.exe", ex);
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
