using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Restall.Services;

public class FileExtractionService : IFileExtractionService
{
    public string? ExtractFiles(string? targetPath = null, string[]? targetFiles = null, string? destinationPath = null)
    {
        const string errorMessage = "Unable to start 7zip process or extract files. Please ensure 7z.exe (Windows) or 7ssz (Linux) is present in Tools/7z";

        targetFiles ??= ["ReShade64.dll", "ReShade32.dll"];
        destinationPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
        targetPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadCache",
            "ReShade_Setup_6.7.2_Addon.exe");

        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath!);
        }

        if (!File.Exists(targetPath))
        {
            return "File for extraction not found. Try again.";
        }

        var sevenZipPath = GetSevenZipPath();

        if (sevenZipPath == null)
        {
            return "Only Windows and Linux are supported.";
        }

        var fileList = string.Join(" ", targetFiles.Select(f => $"\"{f}\""));

        var startInfo = new ProcessStartInfo
        {
            FileName = sevenZipPath,
            Arguments = $"e \"{targetPath}\" -o\"{destinationPath}\" {fileList} -y",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            return errorMessage;
        }

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            return errorMessage;
        }

        return "Successfully extracted files.";
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
            catch { }
        }

        return null;
    }

    private void EnsureExecutable(string path)
    {
        if (!File.Exists(path)) return;

        if (IsExecutable(path)) return;

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
        catch { }
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
            return false;
        }
    }
}
