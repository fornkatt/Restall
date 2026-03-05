using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public interface IModInstallService
{
    Task<Game> InstallModAsync<T>(Game game, T modToInstall) where T: class;
    Task<ModInstallService.UninstallResult> UninstallModAsync<T>(Game game, T modToUninstall) where T: class;
    Task<Game> RemoveOtherReShadeFiles(Game game);
}