using PeNet.Header.Resource;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

public class ModDetectionService : IModDetectionService
{
    private readonly ILogService _logService;

    internal const long DllScanMaxBytes = 10 * 1024 * 1024;

    public ModDetectionService(
        ILogService logService
        )
    {
        _logService = logService;
    }

    public async Task<HashSet<ReShade>?> DetectInstalledReShadeAsync(string executablePath)
    {
        var fileList = new HashSet<ReShade>();

        await ScanFilesAsync(executablePath, ["*.dll", "*.asi"], DllScanMaxBytes, async (file, versionInfo) =>
        {
            if (!string.IsNullOrWhiteSpace(versionInfo.ProductName) &&
                versionInfo.ProductName.Equals("ReShade", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(versionInfo.ProductVersion))
            {
                fileList.Add(new ReShade
                {
                    SelectedFilename = Path.GetFileName(file),
                    Version = versionInfo.ProductVersion,
                    BranchName = ReShade.Branch.Stable,
                    Arch = versionInfo.OriginalFilename?.Contains("64") == true
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

        await ScanFilesAsync(executablePath, ["*.addon64", "*.addon32"], long.MaxValue, async (file, versionInfo) =>
        {
            if (!string.IsNullOrWhiteSpace(versionInfo.OriginalFilename) &&
                versionInfo.OriginalFilename.StartsWith("renodx-", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(versionInfo.FileVersion))
            {
                fileList.Add(new RenoDX
                {
                    SelectedName = Path.GetFileName(file),
                    OriginalName = versionInfo.OriginalFilename,
                    BranchName = RenoDX.Branch.Snapshot, // Assume Snapshot for detected mods not installed by this app
                    Version = ParseRenoDXVersion(versionInfo.FileVersion),
                    Arch = versionInfo.OriginalFilename.Contains("64") == true
                        ? RenoDX.Architecture.x64
                        : RenoDX.Architecture.x32
                });
                await _logService.LogInfoAsync($"Found RenoDX as: {file}");
            }
        });

        return fileList;
    }

    private async Task ScanFilesAsync(
        string path,
        string[] patterns,
        long maxScanBytes,
        Func<string, StringTable, Task> handler)
    {
        var files = patterns
            .SelectMany(p => Directory.GetFiles(path, p))
            .ToArray();

        foreach (var file in files)
        {
            try
            {
                var versionInfo = PeVersionHelper.GetVersionInfo(file, maxScanBytes);
                if (versionInfo is null) continue;
                await handler(file, versionInfo);
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

            var versionInfo = PeVersionHelper.GetVersionInfo(filePath);
            if (versionInfo is null) return null;
            
            return ParseRenoDXVersion(versionInfo.FileVersion);
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
        var match = RegexHelper.RenoDXVersionRegex.Match(fileVersion);
        return match.Success ? match.Groups[1].Value + match.Groups[2].Value : null;
    }
}