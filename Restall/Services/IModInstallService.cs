using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public interface IModInstallService
{
    const string RenoDXUrl = "https://github.com/clshortfuse/renodx/wiki/Mods/";
    const string RenoDXTagUrl = "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd
    const string ReShadeStartUrl = "https://reshade.me/downloads/ReShade_Setup_";
    const string ReShadeEndUrl = "_Addon.exe";
    
    Task<Game> InstallModAsync<T>(Game game, T modToInstall) where T: class;
    Task<ModInstallService.UninstallResult> UninstallModAsync<T>(Game game, T modToUninstall) where T: class;
    Task UpdateModAsync<T>();
    Task<Game> RemoveOtherReShadeFiles(Game game);
}