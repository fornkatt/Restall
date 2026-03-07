using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IModInstallService
{
    Task<Game> InstallModAsync<T>(Game game, T modToInstall) where T: class;
    Task<UninstallResultDto> UninstallReShadeAsync(Game game);
    Task<UninstallResultDto> UninstallRenoDXAsync(Game game);
    Task<Game> RemoveAllReShadeFiles(Game game);
    Task<Game> RemoveAllRenoDXFiles(Game game);
}