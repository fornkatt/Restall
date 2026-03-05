using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IModInstallService
{
    Task<Game> InstallModAsync<T>(Game game, T modToInstall) where T: class;
    Task<UninstallResultDto> UninstallModAsync<T>(Game game, T modToUninstall) where T: class;
    Task<Game> RemoveOtherReShadeFiles(Game game);
}