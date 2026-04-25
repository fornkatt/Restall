using PeNet.Header.Resource;
using Restall.Application.Common;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

internal sealed class ModDetectionService : IModDetectionService
{
    private readonly ILogService _logService;

    private const long s_dllScanMaxBytes = 10 * 1024 * 1024;

    public ModDetectionService(
        ILogService logService
    )
    {
        _logService = logService;
    }

    public async Task<Result<HashSet<ReShade>>> DetectInstalledReShadeAsync(string executablePath)
    {
        var fileList = new HashSet<ReShade>();

        try
        {
            await ScanFilesAsync(executablePath, ["*.dll", "*.asi"], s_dllScanMaxBytes, async (file, versionInfo) =>
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
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<HashSet<ReShade>>.Error("Permission denied scanning directory.", ErrorType.PermissionDenied,
                ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            return Result<HashSet<ReShade>>.Error("Game directory not found.", ErrorType.FileSystemError, ex);
        }
        catch (IOException ex)
        {
            return Result<HashSet<ReShade>>.Error("Failed to scan game directory.", ErrorType.FileSystemError, ex);
        }

        return Result<HashSet<ReShade>>.Success(fileList);
    }

    public async Task<Result<HashSet<RenoDX>>> DetectInstalledRenoDXAsync(string executablePath)
    {
        var fileList = new HashSet<RenoDX>();

        try
        {
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
                        BranchName =
                            RenoDX.Branch.Snapshot, // Assume Snapshot for detected mods not installed by this app
                        Version = ParseRenoDXVersion(versionInfo.FileVersion),
                        Arch = versionInfo.OriginalFilename.Contains("64")
                            ? RenoDX.Architecture.x64
                            : RenoDX.Architecture.x32
                    });
                    await _logService.LogInfoAsync($"Found RenoDX as: {file}");
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<HashSet<RenoDX>>.Error("Permission denied scanning directory.", ErrorType.PermissionDenied,
                ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            return Result<HashSet<RenoDX>>.Error("Game directory not found.", ErrorType.FileSystemError, ex);
        }
        catch (IOException ex)
        {
            return Result<HashSet<RenoDX>>.Error("Failed to scan game directory.", ErrorType.FileSystemError, ex);
        }

        return Result<HashSet<RenoDX>>.Success(fileList);
    }

    public Result<string?> GetRenoDXFileVersion(string filePath)
    {
        var versionInfo = PeVersionHelper.GetVersionInfo(filePath);

        if (versionInfo is { IsSuccess: false })
            return Result<string?>.Error(versionInfo.ErrorMessage, versionInfo.ErrorType, versionInfo.Exception);

        return Result<string?>.Success(ParseRenoDXVersion(versionInfo.Value?.FileVersion));
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
            var versionInfo = PeVersionHelper.GetVersionInfo(file, maxScanBytes);
            switch (versionInfo)
            {
                case { IsSuccess: false }:
                    await _logService.LogErrorAsync(versionInfo.ErrorMessage ?? $"Failed to read {file}",
                        versionInfo.Exception);
                    continue;
                case { IsSuccess: true, Value: not null }:
                    await handler(file, versionInfo.Value);
                    break;
            }
        }
    }

    private static string? ParseRenoDXVersion(string? fileVersion)
    {
        if (string.IsNullOrWhiteSpace(fileVersion)) return null;
        var match = RegexHelper.RenoDXVersionRegex.Match(fileVersion);
        return match.Success ? match.Groups[1].Value + match.Groups[2].Value : null;
    }
}