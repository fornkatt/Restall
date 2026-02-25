using System.Threading.Tasks;

namespace Restall.Services;

public interface IModInstallService
{
    const string RenoDXUrl = "https://github.com/clshortfuse/renodx/wiki/Mods/";
    const string RenoDXTagUrl = "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd
    const string ReShadeStartUrl = "https://reshade.me/downloads/ReShade_Setup_";
    const string ReShadeEndUrl = "_Addon.exe";
    
    Task UninstallModAsync<T>();
    Task InstallModAsync<T>();
    Task UpdateModAsync<T>();
}