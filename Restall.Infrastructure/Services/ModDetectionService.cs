using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Services;

public class ModDetectionService : IModDetectionService
{
    private readonly ILogService _logService;

    private static readonly Regex s_renoDxVersionRegex =
        new(@"^\d+\.(\d{4})\.(\d{4})\.\d+$", RegexOptions.Compiled);

    public ModDetectionService(
        ILogService logService
        )
    {
        _logService = logService;
    }

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
                await _logService.LogInfoAsync($"Found ReShade as: {file}");
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
                    BranchName = RenoDX.Branch.Snapshot, // Assume Snapshot for detected mods not installed by this app
                    Version = ParseRenoDXVersion(fileInfo.FileVersion)
                });
                await _logService.LogInfoAsync($"Found RenoDX as: {file}");
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
                await _logService.LogErrorAsync($"Failed to read file {file}: ", ex);
            }
        }
    }

    public string? GetRenoDXFileVersion(string filePath)
    {
        try
        {
            var fileInfo = FileVersionInfo.GetVersionInfo(filePath);
            return ParseRenoDXVersion(fileInfo.FileVersion);
        }
        catch (Exception ex)
        {
            _logService.LogError("Failed to parse RenoDX file version.", ex);
            return null;
        }
    }

    private static string? ParseRenoDXVersion(string? fileVersion)
    {
        if (string.IsNullOrWhiteSpace(fileVersion)) return null;
        var match = s_renoDxVersionRegex.Match(fileVersion);
        return match.Success ? match.Groups[1].Value + match.Groups[2].Value : null;
    }
}