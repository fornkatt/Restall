using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.UI.Services;

public class ModDetectionService(ILogService logService) : IModDetectionService
{
    public async Task<HashSet<ReShade>?> DetectInstalledReShadeAsync(string executablePath)
    {
        var fileList = new HashSet<ReShade>();

        await ScanFilesAsync(executablePath, ["*.dll", "*.asi"], async (file, fileInfo) =>
        {
            if (!string.IsNullOrWhiteSpace(fileInfo.ProductName) &&
                fileInfo.ProductName.Equals("ReShade", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(fileInfo.ProductVersion))
            {
                fileList.Add(new ReShade
                {
                    SelectedFileName = fileInfo.FileName,
                    Version = fileInfo.ProductVersion,
                    Arch = fileInfo.OriginalFilename!.Contains("64")
                        ? ReShade.Architecture.x64
                        : ReShade.Architecture.x32
                });
                await logService.LogInfoAsync($"Found ReShade as: {file}");
            }
        });

        return fileList;
    }

    public async Task<HashSet<RenoDX>?> DetectInstalledRenoDXAsync(string executablePath)
    {
        var fileList = new HashSet<RenoDX>();

        await ScanFilesAsync(executablePath, ["*.addon64", "*.addon32"], async (file, fileInfo) =>
        {
            if (!string.IsNullOrEmpty(fileInfo.OriginalFilename) &&
                fileInfo.OriginalFilename.StartsWith("renodx-", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(fileInfo.FileVersion))
            {
                fileList.Add(new RenoDX
                {
                    Name = fileInfo.OriginalFilename,
                    Version = fileInfo.FileVersion
                });
                await logService.LogInfoAsync($"Found RenoDX as: {file}");
            }
        });

        return fileList;
    }

    private async Task ScanFilesAsync(
        string path,
        string[] patterns,
        Func<string, FileVersionInfo, Task> handler)
    {
        var files = patterns
            .SelectMany(p => Directory.GetFiles(path, p))
            .ToArray();

        foreach (var file in files)
        {
            try
            {
                var fileInfo = FileVersionInfo.GetVersionInfo(file);
                await handler(file, fileInfo);
            }
            catch (Exception ex)
            {
                await logService.LogErrorAsync($"Failed to read file {file}: ", ex);
            }
        }
    }
}