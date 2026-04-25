using Restall.Application.Common;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

internal sealed class ModInstallService : IModInstallService
{
    private readonly ILogService _logService;
    private readonly IFileService _fileService;

    public ModInstallService(
        IFileService fileService,
        ILogService logService)
    {
        _fileService = fileService;
        _logService = logService;
    }

    public async Task<Result<Game>> InstallModAsync<T>(Game game, T modToInstall, string sourcePath) where T : class
    {
        try
        {
            switch (modToInstall)
            {
                case ReShade reShade:
                {
                    var destinationPath = Path.Combine(game.ExecutablePath!, reShade.SelectedFilename);
                    File.Copy(sourcePath, destinationPath, true);
                    game.ReShade = reShade;
                    await _logService.LogInfoAsync($"Successfully installed ReShade as " +
                                                   $"{reShade.SelectedFilename} to {game.ExecutablePath}");
                    break;
                }
                case RenoDX renoDX:
                {
                    var destinationPath = Path.Combine(game.ExecutablePath!, renoDX.SelectedName!);
                    File.Copy(sourcePath, destinationPath, true);
                    game.RenoDX = renoDX;
                    await _logService.LogInfoAsync($"Successfully installed RenoDX as " +
                                                   $"{renoDX.SelectedName} to {game.ExecutablePath}");
                    break;
                }
            }

            return Result<Game>.Ok(game);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<Game>.Err($"Access denied writing to {game.ExecutablePath}. Game might be running.",
                ResultError.PermissionDenied, ex);
        }
        catch (IOException ex)
        {
            return Result<Game>.Err("Install failed. Disk may be full or the game folder was moved.",
                ResultError.FileSystemError, ex);
        }
    }

    public Result<Game> UninstallReShade(Game game)
    {
        var expectedPath = Path.Combine(game.ExecutablePath!, game.ReShade!.SelectedFilename);
        var deleted = _fileService.TryDeleteFile(expectedPath);

        if (!deleted.IsSuccess)
            return Result<Game>.Err(deleted.ErrorMessage, deleted.Error, deleted.Exception);

        game.ReShade = null;
        return Result<Game>.Ok(game);
    }

    public Result<Game> UninstallRenoDX(Game game)
    {
        var expectedPath = Path.Combine(game.ExecutablePath!, game.RenoDX!.SelectedName!);
        var deleted = _fileService.TryDeleteFile(expectedPath, verifyOriginalFilename: "renodx-");

        if (!deleted.IsSuccess)
            return Result<Game>.Err(deleted.ErrorMessage, deleted.Error, deleted.Exception);

        game.RenoDX = null;
        return Result<Game>.Ok(game);
    }

    public async Task<Result<Game>> RemoveAllReShadeFilesAsync(Game game)
    { 
        var files = Directory.GetFiles(game.ExecutablePath!, "*.dll")
                .Concat(Directory.GetFiles(game.ExecutablePath!, "*.asi"));
    
        var removedCount = 0;
    
        foreach (var file in files)
        {
            var versionInfo = PeVersionHelper.GetVersionInfo(file);
    
            if (!versionInfo.IsSuccess)
            {
                await _logService.LogErrorAsync(versionInfo.ErrorMessage ?? $"Failed to read {file}", versionInfo.Exception);
                continue;
            }
            
            if (versionInfo.Value?.ProductName?.Equals("ReShade", StringComparison.OrdinalIgnoreCase) == true)
            {
                var deleted = _fileService.TryDeleteFile(file);
    
                if (deleted.IsSuccess)
                {
                    removedCount++;
                    await _logService.LogInfoAsync($"Removed ReShade file: {Path.GetFileName(file)}");
                }
                else
                {
                    await _logService.LogInfoAsync(deleted.ErrorMessage ?? $"Failed to delete file at expected location: {Path.GetFileName(file)}");
                }
            }
        }
    
        await _logService.LogInfoAsync(removedCount > 0
            ? $"Successfully removed {removedCount} ReShade files."
            : "No ReShade files found to uninstall.");
    
        game.ReShade = null;
        return Result<Game>.Ok(game);
    }
    
    public async Task<Result<Game>> RemoveAllRenoDXFilesAsync(Game game)
    {
        var files = Directory.GetFiles(game.ExecutablePath!, "*.addon32")
                .Concat(Directory.GetFiles(game.ExecutablePath!, "*.addon64"));
    
        var removedCount = 0;
    
        foreach (var file in files)
        {
            var versionInfo = PeVersionHelper.GetVersionInfo(file);

            if (!versionInfo.IsSuccess)
            {
                await _logService.LogErrorAsync(versionInfo.ErrorMessage ?? $"Failed to read {file}", versionInfo.Exception);
                continue;
            }
            
            if (versionInfo.Value?.OriginalFilename?.StartsWith("renodx-", StringComparison.OrdinalIgnoreCase) == true)
            {
                var deleted = _fileService.TryDeleteFile(file);
    
                if (deleted.IsSuccess)
                {
                    removedCount++;
                    await _logService.LogInfoAsync($"Removed RenoDX file: {Path.GetFileName(file)}");
                }
                else
                {
                    await _logService.LogErrorAsync(deleted.ErrorMessage ??
                                                   $"Failed to delete file at expected location: {Path.GetFileName(file)}", deleted.Exception);
                }
            }
        }
    
        await _logService.LogInfoAsync(removedCount > 0
            ? $"Successfully removed {removedCount} RenoDX files."
            : "No RenoDX files found to remove.");
    
        game.RenoDX = null;
        return Result<Game>.Ok(game);
    }
}