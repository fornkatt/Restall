using Microsoft.Extensions.Logging;
using PeNet.Header.Resource;
using Restall.Application.Common;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Logging;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

internal sealed partial class ModDetectionService : IModDetectionService
{
    private const long s_dllScanMaxBytes = 10 * 1024 * 1024;
    private readonly ILogger<ModDetectionService> _logger;

    public ModDetectionService(
        ILogger<ModDetectionService> logger
    )
    {
        _logger = logger;
    }

    // TODO: swap actual mod classes to DTO
    public async Task<Result<HashSet<ReShade>>> DetectInstalledReShadeAsync(string executableDirectory)
    {
        LogModDetectionStart("ReShade", executableDirectory);

        HashSet<ReShade> fileList = [];

        try
        {
            await ScanFilesAsync(executableDirectory, ["*.dll", "*.asi"], s_dllScanMaxBytes,
                async (file, versionInfo) =>
                {
                    if (!string.IsNullOrWhiteSpace(versionInfo.ProductName) &&
                        versionInfo.ProductName.Equals("ReShade", StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(versionInfo.ProductVersion))
                    {
                        var filename = Path.GetFileName(file);

                        fileList.Add(new ReShade
                        {
                            SelectedFilename = filename,
                            Version = versionInfo.ProductVersion,
                            BranchName = ReShade.Branch.Stable,
                            Arch = versionInfo.OriginalFilename?.Contains("64") == true
                                ? ReShade.Architecture.x64
                                : ReShade.Architecture.x32
                        });
                        LogModFound("ReShade", filename, executableDirectory);
                    }
                });

            // TODO: multiple of the same mod is usually an anomaly. Redo later when handling for this lands
            LogModDetectionFinished("ReShade", executableDirectory, fileList.Count);

            return Result<HashSet<ReShade>>.Success(fileList);
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
    }

    public async Task<Result<HashSet<RenoDX>>> DetectInstalledRenoDXAsync(string executableDirectory)
    {
        LogModDetectionStart("RenoDX", executableDirectory);

        HashSet<RenoDX> fileList = [];

        try
        {
            await ScanFilesAsync(executableDirectory, ["*.addon64", "*.addon32"], long.MaxValue,
                async (file, versionInfo) =>
                {
                    if (!string.IsNullOrWhiteSpace(versionInfo.OriginalFilename) &&
                        versionInfo.OriginalFilename.StartsWith("renodx-", StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(versionInfo.FileVersion))
                    {
                        var filename = Path.GetFileName(file);

                        fileList.Add(new RenoDX
                        {
                            SelectedName = filename,
                            OriginalName = versionInfo.OriginalFilename,
                            BranchName =
                                RenoDX.Branch.Snapshot, // Assume Snapshot for detected mods not installed by this app
                            Version = ParseRenoDXVersion(versionInfo.FileVersion),
                            Arch = versionInfo.OriginalFilename.Contains("64")
                                ? RenoDX.Architecture.x64
                                : RenoDX.Architecture.x32
                        });
                        LogModFound("RenoDX", filename, executableDirectory);
                    }
                });

            // TODO: multiple of the same mod is usually an anomaly. Redo later when handling for this lands
            LogModDetectionFinished("RenoDX", executableDirectory, fileList.Count);

            return Result<HashSet<RenoDX>>.Success(fileList);
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
    }

    public Result<string?> GetRenoDXFileVersion(string filePath)
    {
        var versionInfo = PeVersionHelper.GetVersionInfo(filePath);

        if (versionInfo is null)
            return Result<string?>.Error($"Could not read file {filePath}", ErrorType.FileSystemError);

        return Result<string?>.Success(ParseRenoDXVersion(versionInfo.FileVersion));
    }

    // TODO: make synchronous
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
            try
            {
                var versionInfo = PeVersionHelper.GetVersionInfo(file, maxScanBytes);

                if (versionInfo is null)
                    continue;

                await handler(file, versionInfo);
            }
            catch (Exception ex)
            {
                _logger.PeFileReadFailure(file, ex);
                // Protect the scanner
            }
    }

    private static string? ParseRenoDXVersion(string? fileVersion)
    {
        if (string.IsNullOrWhiteSpace(fileVersion)) return null;
        var match = RegexHelper.RenoDXVersionRegex.Match(fileVersion);
        return match.Success ? match.Groups[1].Value + match.Groups[2].Value : null;
    }
}