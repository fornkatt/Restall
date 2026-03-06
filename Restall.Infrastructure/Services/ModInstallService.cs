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

    public async Task<UninstallResultDto> UninstallModAsync<T>(Game game, T modToUninstall) where T : class
    {
        var result = new UninstallResultDto { UpdatedGame = game };
        
        switch  (modToUninstall)
        {
            case ReShade:
            {
                if (!Directory.Exists(game.ExecutablePath))
                {
                    await _logService.LogErrorAsync($"Game executable path not found: {game.ExecutablePath}. " +
                                                   $"Please perform a library rescan.");
                    return result;
                }
                
                string expectedPath = Path.Combine(game.ExecutablePath, game.ReShade!.SelectedFileName);
    
                if (File.Exists(expectedPath))
                {
                    File.Delete(expectedPath);
                    await _logService.LogInfoAsync($"Removed ReShade file: {expectedPath}");
                    game.ReShade = null;
                }
                else
                {
                    await _logService.LogWarningAsync($"ReShade file not found at expected location: {expectedPath}");
                    game.ReShade = null;
                    result.ShouldPromptForDeepScan = true;
                }

                break;
            }
            case RenoDX:
            {
                string expectedPath = Path.Combine(game.ExecutablePath!, game.RenoDX!.Name!);
                
                if (!File.Exists(expectedPath))
                {
                    await _logService.LogErrorAsync($"RenoDX not found at expected location: {expectedPath}. " +
                                                   $"Clearing RenoDX anyway.");
                    game.RenoDX = null;
                }
                else
                {
                    try
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(expectedPath);
                        string? originalFilename = versionInfo.OriginalFilename;
                        
                        if (originalFilename?.StartsWith("renodx-", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            File.Delete(expectedPath);
                            await _logService.LogInfoAsync($"Removed RenoDX file: {expectedPath}");
                        }
                        else
                        {
                            await _logService.LogWarningAsync($"File at {expectedPath} does not appear to be a RenoDX addon " +
                                                             $"(OriginalFilename: '{originalFilename}'). Skipping deletion.");
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logService.LogErrorAsync($"Failed to verify RenoDX file at {expectedPath}", ex);
                    }

                    game.RenoDX = null;
                }

                break;
            }
        }

        return result;
    }

    public async Task<Game> RemoveOtherReShadeFiles(Game game)
    {
        var dllFiles = Directory.GetFiles(game.ExecutablePath!, "*.dll")
            .Concat(Directory.GetFiles(game.ExecutablePath!, "*.asi"))
            .ToArray();
        int removedCount = 0;

        foreach (var dllFile in dllFiles)
        {
            try
            {
                var fileInfo = FileVersionInfo.GetVersionInfo(dllFile);

                if (fileInfo.ProductName?.Equals("ReShade", StringComparison.OrdinalIgnoreCase) == true)
                {
                    File.Delete(dllFile);
                    removedCount++;
                    await _logService.LogInfoAsync($"Removed ReShade file: {Path.GetFileName(dllFile)}");
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Failed to remove {Path.GetFileName(dllFile)}", ex);
            }
        }

        if (removedCount > 0)
        {
            await _logService.LogInfoAsync($"Successfully removed {removedCount} ReShade files.");
        }
        else
        {
            await _logService.LogInfoAsync("No ReShade files found to uninstall.");
        }
        
        game.ReShade = null;
        return game;
    }
}