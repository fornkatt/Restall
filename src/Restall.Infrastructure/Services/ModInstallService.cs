using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

internal sealed class ModInstallService : IModInstallService
{
    private readonly ILogService _logService;

    public ModInstallService(
        ILogService logService)
    {
        _logService = logService;
    }

    public async Task<ModOperationResultDto> InstallModAsync<T>(Game game, T modToInstall, string sourcePath) where T : class
    {
        try
        {
            switch (modToInstall)
            {
                case ReShade reShade:
                {
                    if (game.ReShade is not null)
                        await TryDeleteFileAsync(Path.Combine(game.ExecutablePath!, game.ReShade.SelectedFilename));

                    string destinationPath = Path.Combine(game.ExecutablePath!, reShade.SelectedFilename);
                    File.Copy(sourcePath, destinationPath, true);
                    game.ReShade = reShade;
                    await _logService.LogInfoAsync($"Successfully installed ReShade as " +
                                                  $"{reShade.SelectedFilename} to {game.ExecutablePath}");
                    break;
                }
                case RenoDX renoDX:
                {
                    if (renoDX.OriginalName is not null && renoDX.OriginalName != renoDX.SelectedName)
                        await TryDeleteFileAsync(Path.Combine(game.ExecutablePath!, renoDX.OriginalName), verifyOriginalFilename: "renodx-");

                    string destinationPath = Path.Combine(game.ExecutablePath!, renoDX.SelectedName!);
                    File.Copy(sourcePath, destinationPath, true);
                    game.RenoDX = renoDX;
                    await _logService.LogInfoAsync($"Successfully installed RenoDX as " +
                                                  $"{renoDX.SelectedName} to {game.ExecutablePath}");
                    break;
                }
            }

            return new ModOperationResultDto(true, game, "Installation successful!");
        }
        catch (UnauthorizedAccessException ex)
        {
            await _logService.LogErrorAsync($"Access denied writing to {game.ExecutablePath}. Please make sure the game is not running.", ex);
            return new ModOperationResultDto(false, game, "Access denied. Please make sure the game is not running and try again.");
        }
        catch (IOException ex)
        {
            await _logService.LogErrorAsync($"IO failure during install.", ex);
            return new ModOperationResultDto(false, game, "Install failed. Disk may be full or the game folder was moved.");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Unexpected error occured during installation.", ex);
            return new ModOperationResultDto(false, game, "An unexpected error occured. Check the log in the 'Logs' folder for details.");
        }
    }

    public async Task<ModOperationResultDto> UninstallReShadeAsync(Game game)
    {
        string expectedPath = Path.Combine(game.ExecutablePath!, game.ReShade!.SelectedFilename);
        bool deleted = await TryDeleteFileAsync(expectedPath);

        game.ReShade = null;

        return deleted
            ? new ModOperationResultDto(true, game, "Uninstalled!")
            : new ModOperationResultDto(false, game, """
            Uninstall failed. Please ensure all files are removed from the game directory.
            Or perform a full rescan to pick up stray .dll or .asi ReShade files.
            """, true);
    }

    public async Task<ModOperationResultDto> UninstallRenoDXAsync(Game game)
    {
        string expectedPath = Path.Combine(game.ExecutablePath!, game.RenoDX!.SelectedName!);
        bool deleted = await TryDeleteFileAsync(expectedPath, verifyOriginalFilename: "renodx-");

        game.RenoDX = null;

        return deleted
            ? new ModOperationResultDto(true, game, "Uninstalled!")
            : new ModOperationResultDto(false, game, """
            Uninstall failed. Please ensure all files are removed from the game directory.
            Or perform a full rescan to pick up stray .addon32 or .addon64 RenoDX files.
            """, true);
    }

    private async Task<bool> TryDeleteFileAsync(string path, string? verifyOriginalFilename = null)
    {
        if (!File.Exists(path))
        {
            await _logService.LogWarningAsync($"File not found at expected location: {path}");
            return false;
        }

        try
        {
            if (verifyOriginalFilename is not null)
            {
                var originalFilename = PeVersionHelper.GetVersionInfo(path)?.OriginalFilename;

                if (originalFilename?.StartsWith(verifyOriginalFilename, StringComparison.OrdinalIgnoreCase) != true)
                {
                    await _logService.LogWarningAsync($"File at {path} did not match expected prefix '{verifyOriginalFilename}' (was: '{originalFilename}'). Skipping deletion.");
                    return false;
                }
            }

            File.Delete(path);
            await _logService.LogInfoAsync($"Removed file: {path}");
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            await _logService.LogErrorAsync($"Access denied trying to delete {path}. Please ensure the game is not running and try again.", ex);
            return false;
        }
        catch (IOException ex)
        {
            await _logService.LogErrorAsync($"File locked at {path}. Could not delete.", ex);
            return false;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Unexpected error occured. Failed to delete file at {path}", ex);
            return false;
        }
    }

    public async Task<Game> RemoveAllReShadeFiles(Game game)
    {
        var files = Directory.GetFiles(game.ExecutablePath!, "*.dll")
            .Concat(Directory.GetFiles(game.ExecutablePath!, "*.asi"));

        int removedCount = 0;

        foreach (var file in files)
        {
            try
            {
                var versionInfo = PeVersionHelper.GetVersionInfo(file, long.MaxValue);

                if (versionInfo?.ProductName?.Equals("ReShade", StringComparison.OrdinalIgnoreCase) == true)
                {
                    File.Delete(file);
                    removedCount++;
                    await _logService.LogInfoAsync($"Removed ReShade file: {Path.GetFileName(file)}");
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Failed to remove {Path.GetFileName(file)}", ex);
            }
        }

        await _logService.LogInfoAsync(removedCount > 0
            ? $"Successfully removed {removedCount} ReShade files."
            : "No ReShade files found to uninstall.");

        game.ReShade = null;
        return game;
    }

    public async Task<Game> RemoveAllRenoDXFiles(Game game)
    {
        var files = Directory.GetFiles(game.ExecutablePath!, "*.addon32")
            .Concat(Directory.GetFiles(game.ExecutablePath!, "*.addon64"));

        int removedCount = 0;

        foreach (var file in files)
        {
            try
            {
                var versionInfo = PeVersionHelper.GetVersionInfo(file, long.MaxValue);

                if (versionInfo?.OriginalFilename?.StartsWith("renodx-", StringComparison.OrdinalIgnoreCase) == true)
                {
                    File.Delete(file);
                    removedCount++;
                    await _logService.LogInfoAsync($"Removed RenoDX file: {Path.GetFileName(file)}");
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Failed to remove {Path.GetFileName(file)}", ex);
            }
        }

        await _logService.LogInfoAsync(removedCount > 0
            ? $"Successfully removed {removedCount} RenoDX files."
            : $"No RenoDX files found to remove.");

        game.RenoDX = null;
        return game;
    }
}