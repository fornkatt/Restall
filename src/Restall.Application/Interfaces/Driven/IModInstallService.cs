using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IModInstallService
{
    Task<ModOperationResultDto> InstallModAsync<T>(Game game, T modToInstall, string sourcePath) where T: class;
    Task<ModOperationResultDto> UninstallReShadeAsync(Game game);
    Task<ModOperationResultDto> UninstallRenoDXAsync(Game game);
    Task<Game> RemoveAllReShadeFiles(Game game);
    Task<Game> RemoveAllRenoDXFiles(Game game);
}