using System.Diagnostics;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

public class ModInstallService : IModInstallService
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
            if (!Directory.Exists(game.ExecutablePath))
            {
                return new ModOperationResultDto(false, game);
            }

            switch (modToInstall)
            {
                case ReShade reShade:
                {
                    string destinationPath = Path.Combine(game.ExecutablePath, reShade.SelectedFileName);

                    File.Copy(sourcePath, destinationPath, true);

                    game.ReShade = reShade;

                    await _logService.LogInfoAsync($"Successfully installed ReShade as " +
                                                  $"{reShade.SelectedFileName} to {game.ExecutablePath}");

                    break;
                }
                case RenoDX renoDx:
                {
                    string destinationPath = Path.Combine(game.ExecutablePath, renoDx.SelectedName!);

                    File.Copy(sourcePath, destinationPath, true);

                    game.RenoDX = renoDx;

                    await _logService.LogInfoAsync($"Successfully installed RenoDX as " +
                                                  $"{renoDx.SelectedName} to {game.ExecutablePath}");

                    break;
                }
            }

            return new ModOperationResultDto(true, game);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Could not install mod.", ex);
            return new ModOperationResultDto(false, game, $"Failed to install mod.");
        }
    }

    public async Task<ModOperationResultDto> UninstallReShadeAsync(Game game)
    {
        if (!Directory.Exists(game.ExecutablePath))
        {
            await _logService.LogErrorAsync($"Game executable path not found: {game.ExecutablePath}. Please perform a library rescan.");
            return new ModOperationResultDto(false, game);
        }

        string expectedPath = Path.Combine(game.ExecutablePath, game.ReShade!.SelectedFileName);
        bool deleted = await TryDeleteFileAsync(expectedPath);

        game.ReShade = null;
        return new ModOperationResultDto(deleted, game, ShouldPromptForDeepScan: !deleted);
    }

    public async Task<ModOperationResultDto> UninstallRenoDXAsync(Game game)
    {
        string expectedPath = Path.Combine(game.ExecutablePath!, game.RenoDX!.SelectedName!);

        if (!File.Exists(expectedPath))
        {
            await _logService.LogErrorAsync($"RenoDX not found at expected location: {expectedPath}.");
            game.RenoDX = null;
            return new ModOperationResultDto(false, game, null, true);
        }

        bool deleted = await TryDeleteFileAsync(expectedPath, verifyOriginalFilename: "renodx-");

        game.RenoDX = null;
        return new ModOperationResultDto(deleted, game, ShouldPromptForDeepScan: !deleted);
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
                var originalFilename = FileVersionInfo.GetVersionInfo(path).OriginalFilename;

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
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to delete file at {path}", ex);
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
                var fileInfo = FileVersionInfo.GetVersionInfo(file);

                if (fileInfo.ProductName?.Equals("ReShade", StringComparison.OrdinalIgnoreCase) == true)
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
                var fileInfo = FileVersionInfo.GetVersionInfo(file);

                if (fileInfo.OriginalFilename?.StartsWith("renodx-", StringComparison.OrdinalIgnoreCase) == true)
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