// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

internal sealed class ModInstallService : IModInstallService
{
    private readonly IFileService _fileService;

    public ModInstallService(
        IFileService fileService
    )
    {
        _fileService = fileService;
    }

    // TODO: mod enum instead of T class?
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
                        break;
                    }
                case RenoDX renoDX:
                    {
                        var destinationPath = Path.Combine(game.ExecutablePath!, renoDX.SelectedName!);
                        File.Copy(sourcePath, destinationPath, true);
                        game.RenoDX = renoDX;
                        break;
                    }
            }

            return Result<Game>.Success(game);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<Game>.Error($"Access denied writing to {game.ExecutablePath}. Game might be running.",
                ErrorType.PermissionDenied, ex);
        }
        catch (IOException ex)
        {
            return Result<Game>.Error("Install failed. Disk may be full or the game folder was moved.",
                ErrorType.FileSystemError, ex);
        }
    }

    public Result<Game> UninstallReShade(Game game)
    {
        var expectedPath = Path.Combine(game.ExecutablePath!, game.ReShade!.SelectedFilename);
        var deleted = _fileService.TryDeleteFile(expectedPath);

        if (!deleted.IsSuccess)
            return Result<Game>.Error(deleted.Message, deleted.ErrorType, deleted.Exception);

        game.ReShade = null;
        return Result<Game>.Success(game);
    }
}
