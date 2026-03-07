using System.Diagnostics;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

public class ModInstallService : IModInstallService
{
    private readonly ILogService _logService;
    private readonly ICachePathService _cachePathService;

    public ModInstallService(
        ILogService logService,
        ICachePathService cachePathService)
    {
        _logService = logService;
        _cachePathService = cachePathService;
    }

    public async Task<Game> InstallModAsync<T>(Game game, T modToInstall) where T : class
    {
        try
        {
            if (!Directory.Exists(game.ExecutablePath))
            {
                return game;
            }

            switch (modToInstall)
            {
                case ReShade reShade:
                {
                    string cacheFilePath = Path.Combine(_cachePathService.GetReShadeCachePath(reShade), reShade.OriginalFileName);

                    if (!File.Exists(cacheFilePath))
                    {
                        await _logService.LogErrorAsync(
                            $"ReShade cache file not found: {cacheFilePath}. Clear the cache and download again.");
                        return game;
                    }

                    string destinationPath = Path.Combine(game.ExecutablePath, reShade.SelectedFileName);

                    File.Copy(cacheFilePath, destinationPath, true);

                    game.ReShade = reShade;

                    await _logService.LogInfoAsync($"Successfully installed ReShade as " +
                                                  $"{reShade.SelectedFileName} to {game.ExecutablePath}");

                    break;
                }
                case RenoDX renoDx:
                {
                    string cacheFilePath = _cachePathService.GetRenoDXCachePath(renoDx);

                    if (!File.Exists(cacheFilePath))
                    {
                        await _logService.LogErrorAsync(
                            $"RenoDX cache file not found: {cacheFilePath}. Clear the cache and download again.");
                        return game;
                    }

                    string destinationPath = Path.Combine(game.ExecutablePath, renoDx.Name!);

                    File.Copy(cacheFilePath, destinationPath, true);

                    game.RenoDX = renoDx;

                    await _logService.LogInfoAsync($"Successfully installed RenoDX as " +
                                                  $"{renoDx.Name} to {game.ExecutablePath}");

                    break;
                }
            }

            return game;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Could not install mod.", ex);
            return game;
        }
    }

    public async Task<UninstallResultDto> UninstallReShadeAsync(Game game)
    {
        var result = new UninstallResultDto { UpdatedGame = game };

        if (!Directory.Exists(game.ExecutablePath))
        {
            await _logService.LogErrorAsync($"Game executable path not found: {game.ExecutablePath}. Please perform a library rescan.");
            return result;
        }

        string expectedPath = Path.Combine(game.ExecutablePath, game.ReShade!.SelectedFileName);
        bool deleted = await TryDeleteFileAsync(expectedPath);

        if (!deleted)
            result.ShouldPromptForDeepScan = true;

        game.ReShade = null;
        return result;
    }

    public async Task<UninstallResultDto> UninstallRenoDXAsync(Game game)
    {
        var result = new UninstallResultDto { UpdatedGame = game };
        string expectedPath = Path.Combine(game.ExecutablePath!, game.RenoDX!.Name!);
        bool deleted = false;

        if (!File.Exists(expectedPath))
        {
            await _logService.LogErrorAsync($"RenoDX not found at expected location: {expectedPath}.");
            deleted = false;
        }
        else
        {
            deleted = await TryDeleteFileAsync(expectedPath, verifyOriginalFilename: "renodx-");
        }

        if (!deleted)
            result.ShouldPromptForDeepScan = true;

        game.RenoDX = null;
        return result;
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

    //public async Task<UninstallResultDto> UninstallModAsync<T>(Game game, T modToUninstall) where T : class
    //{
    //    var result = new UninstallResultDto { UpdatedGame = game };

    //    switch  (modToUninstall)
    //    {
    //        case ReShade:
    //        {
    //            if (!Directory.Exists(game.ExecutablePath))
    //            {
    //                await _logService.LogErrorAsync($"Game executable path not found: {game.ExecutablePath}. " +
    //                                               $"Please perform a library rescan.");
    //                return result;
    //            }

    //            string expectedPath = Path.Combine(game.ExecutablePath, game.ReShade!.SelectedFileName);

    //            if (File.Exists(expectedPath))
    //            {
    //                File.Delete(expectedPath);
    //                await _logService.LogInfoAsync($"Removed ReShade file: {expectedPath}");
    //                game.ReShade = null;
    //            }
    //            else
    //            {
    //                await _logService.LogWarningAsync($"ReShade file not found at expected location: {expectedPath}");
    //                game.ReShade = null;
    //                result.ShouldPromptForDeepScan = true;
    //            }

    //            break;
    //        }
    //        case RenoDX:
    //        {
    //            string expectedPath = Path.Combine(game.ExecutablePath!, game.RenoDX!.Name!);

    //            if (!File.Exists(expectedPath))
    //            {
    //                await _logService.LogErrorAsync($"RenoDX not found at expected location: {expectedPath}. " +
    //                                               $"Clearing RenoDX anyway.");
    //                game.RenoDX = null;
    //            }
    //            else
    //            {
    //                try
    //                {
    //                    var versionInfo = FileVersionInfo.GetVersionInfo(expectedPath);
    //                    string? originalFilename = versionInfo.OriginalFilename;

    //                    if (originalFilename?.StartsWith("renodx-", StringComparison.OrdinalIgnoreCase) == true)
    //                    {
    //                        File.Delete(expectedPath);
    //                        await _logService.LogInfoAsync($"Removed RenoDX file: {expectedPath}");
    //                    }
    //                    else
    //                    {
    //                        await _logService.LogWarningAsync($"File at {expectedPath} does not appear to be a RenoDX addon " +
    //                                                         $"(OriginalFilename: '{originalFilename}'). Skipping deletion.");
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    await _logService.LogErrorAsync($"Failed to verify RenoDX file at {expectedPath}", ex);
    //                }

    //                game.RenoDX = null;
    //            }

    //            break;
    //        }
    //    }

    //    return result;
    //}
}