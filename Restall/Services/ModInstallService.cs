using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public class ModInstallService(ILogService logService) : IModInstallService
{
    public class UninstallResult
    {
        public Game UpdatedGame { get; set; } = null!;
        public bool ShouldPromptForDeepScan { get; set; }
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
                    string cacheFilePath = Path.Combine(reShade.GetCachePath(), reShade.OriginalFileName);

                    if (!File.Exists(cacheFilePath))
                    {
                        await logService.LogErrorAsync(
                            $"ReShade cache file not found: {cacheFilePath}. Clear the cache and download again.");
                        return game;
                    }

                    string destinationPath = Path.Combine(game.ExecutablePath, reShade.SelectedFileName);

                    File.Copy(cacheFilePath, destinationPath, true);

                    game.ReShade = reShade;

                    await logService.LogInfoAsync($"Successfully installed ReShade as " +
                                                  $"{reShade.SelectedFileName} to {game.ExecutablePath}");

                    break;
                }
                case RenoDX renoDx:
                {
                    string cacheFilePath = renoDx.GetCachePath();

                    if (!File.Exists(cacheFilePath))
                    {
                        await logService.LogErrorAsync(
                            $"RenoDX cache file not found: {cacheFilePath}. Clear the cache and download again.");
                        return game;
                    }

                    string destinationPath = Path.Combine(game.ExecutablePath, renoDx.Name!);

                    File.Copy(cacheFilePath, destinationPath, true);

                    game.RenoDX = renoDx;

                    await logService.LogInfoAsync($"Successfully installed RenoDX as " +
                                                  $"{renoDx.Name} to {game.ExecutablePath}");

                    break;
                }
            }

            return game;
        }
        catch (Exception ex)
        {
            await logService.LogErrorAsync("Could not install mod.", ex);
            return game;
        }
    }

    public async Task<UninstallResult> UninstallModAsync<T>(Game game, T modToUninstall) where T : class
    {
        var result = new UninstallResult { UpdatedGame = game };
        
        switch  (modToUninstall)
        {
            case ReShade:
            {
                if (!Directory.Exists(game.ExecutablePath))
                {
                    await logService.LogErrorAsync($"Game executable path not found: {game.ExecutablePath}. " +
                                                   $"Please perform a library rescan.");
                    return result;
                }
                
                string expectedPath = Path.Combine(game.ExecutablePath, game.ReShade!.SelectedFileName);
    
                if (File.Exists(expectedPath))
                {
                    File.Delete(expectedPath);
                    await logService.LogInfoAsync($"Removed ReShade file: {expectedPath}");
                    game.ReShade = null;
                }
                else
                {
                    await logService.LogWarningAsync($"ReShade file not found at expected location: {expectedPath}");
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
                    await logService.LogErrorAsync($"RenoDX not found at expected location: {expectedPath}. " +
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
                            await logService.LogInfoAsync($"Removed RenoDX file: {expectedPath}");
                        }
                        else
                        {
                            await logService.LogWarningAsync($"File at {expectedPath} does not appear to be a RenoDX addon " +
                                                             $"(OriginalFilename: '{originalFilename}'). Skipping deletion.");
                        }
                    }
                    catch (Exception ex)
                    {
                        await logService.LogErrorAsync($"Failed to verify RenoDX file at {expectedPath}", ex);
                    }

                    game.RenoDX = null;
                }

                break;
            }
        }

        return result;
    }

    public async Task<T> UpdateModAsync<T>(T modToUpdate) where T : class
    {
        throw new NotImplementedException();
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
                var versionInfo = FileVersionInfo.GetVersionInfo(dllFile);

                if (versionInfo.ProductName?.Equals("ReShade", StringComparison.OrdinalIgnoreCase) == true)
                {
                    File.Delete(dllFile);
                    removedCount++;
                    await logService.LogInfoAsync($"Removed ReShade file: {Path.GetFileName(dllFile)}");
                }
            }
            catch (Exception ex)
            {
                await logService.LogErrorAsync($"Failed to remove {Path.GetFileName(dllFile)}", ex);
            }
        }

        if (removedCount > 0)
        {
            await logService.LogInfoAsync($"Successfully removed {removedCount} ReShade files.");
        }
        else
        {
            await logService.LogInfoAsync("No ReShade files found to uninstall.");
        }
        
        game.ReShade = null;
        return game;
    }
}