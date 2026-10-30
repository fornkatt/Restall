// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Logging;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;
using Restall.Infrastructure.DTOs;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

internal sealed partial class ModDetectionService : IModDetectionService
{
    private readonly ILogger<ModDetectionService> _logger;

    public ModDetectionService(
        ILogger<ModDetectionService> logger
    )
    {
        _logger = logger;
    }

    // TODO: swap actual mod classes to DTO

    public Task<Result<HashSet<ReShade>>> DetectInstalledReShadeAsync(string executableDirectory) =>
        Task.FromResult(DetectInstalledReShade(executableDirectory));

    public Task<Result<HashSet<RenoDX>>> DetectInstalledRenoDXAsync(string executableDirectory) =>
        Task.FromResult(DetectInstalledRenoDX(executableDirectory));

    public Result<string?> GetRenoDXFileVersion(string filePath)
    {
        var versionInfo = PeVersionHelper.GetVersionInfo(filePath);

        if (versionInfo is null)
            return Result<string?>.Error($"Could not read file {filePath}", ErrorType.FileSystemError);

        return Result<string?>.Success(ParseRenoDXVersion(versionInfo.FileVersion));
    }

    private Result<HashSet<ReShade>> DetectInstalledReShade(string executableDirectory)
    {
        LogModDetectionStart("ReShade", executableDirectory);

        var fileList = new HashSet<ReShade>();

        try
        {
            foreach (var scannedFile in ScanFiles(executableDirectory, ["*.dll", "*.asi"]))
            {
                var versionInfo = scannedFile.VersionInfo;

                if (!string.IsNullOrWhiteSpace(versionInfo.ProductName)
                    && versionInfo.ProductName.Equals("ReShade", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(versionInfo.ProductVersion))
                {
                    var filename = Path.GetFileName(scannedFile.FilePath);

                    fileList.Add(new ReShade
                    {
                        SelectedFilename = filename,
                        Version = versionInfo.ProductVersion,
                        BranchName = ReShade.Branch.Stable,
                        Arch = versionInfo.OriginalFilename?.Contains("64") == true
                            ? Architecture.X64
                            : Architecture.X32
                    });

                    LogModFound("ReShade", filename, executableDirectory);
                }
            }

            // TODO: multiple of the same mod is usually an anomaly. Redo later when handling for this lands
            LogModDetectionComplete("ReShade", executableDirectory, fileList.Count);

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

    private Result<HashSet<RenoDX>> DetectInstalledRenoDX(string executableDirectory)
    {
        LogModDetectionStart("RenoDX", executableDirectory);

        var fileList = new HashSet<RenoDX>();

        try
        {
            foreach (var scannedFile in ScanFiles(executableDirectory,
                         [$"*{RenoDX.AddonExtension64}", $"*{RenoDX.AddonExtension32}"]))
            {
                var versionInfo = scannedFile.VersionInfo;

                if (!string.IsNullOrWhiteSpace(versionInfo.OriginalFilename)
                    && versionInfo.OriginalFilename.StartsWith(RenoDX.OriginalFilenameStart,
                        StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(versionInfo.FileVersion))
                {
                    var filename = Path.GetFileName(scannedFile.FilePath);

                    fileList.Add(new RenoDX
                    {
                        SelectedName = filename,
                        OriginalName = versionInfo.OriginalFilename,
                        BranchName = RenoDX.Branch.Snapshot,
                        Version = ParseRenoDXVersion(versionInfo.FileVersion),
                        Arch = versionInfo.OriginalFilename.Contains("64")
                            ? Architecture.X64
                            : Architecture.X32
                    });

                    LogModFound("RenoDX", filename, executableDirectory);
                }
            }

            // TODO: multiple of the same mod is usually an anomaly. Redo later when handling for this lands
            LogModDetectionComplete("RenoDX", executableDirectory, fileList.Count);

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

    private List<ScannedFile> ScanFiles(string path, string[] patterns)
    {
        var files = patterns
            .SelectMany(p => Directory.GetFiles(path, p))
            .ToArray();

        var scannedFiles = new List<ScannedFile>();

        foreach (var file in files)
        {
            try
            {
                var versionInfo = PeVersionHelper.GetVersionInfo(file);

                if (versionInfo is not null)
                    scannedFiles.Add(new ScannedFile(file, versionInfo));
            }
            catch (Exception ex)
            {
                _logger.PeFileReadFailure(file, ex);
                // Protect the scanner
            }
        }

        return scannedFiles;
    }

    private static string? ParseRenoDXVersion(string? fileVersion)
    {
        if (string.IsNullOrWhiteSpace(fileVersion))
            return null;

        var match = RegexHelper.RenoDXVersionRegex.Match(fileVersion);

        return match.Success ? match.Groups[1].Value + match.Groups[2].Value : null;
    }

    private sealed record ScannedFile(string FilePath, PeVersionInfo VersionInfo);
}
