using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public class UpdateModService : IUpdateModService
{
    private const string ReShadeStartUrl = "https://reshade.me/downloads/ReShade_Setup_";
    private const string ReShadeEndUrl = "_Addon.exe";
    private const string RenoDxUrl = "https://github.com/clshortfuse/renodx/wiki/Mods/";
    private const string RenoDxTagUrl = "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd
    
    public async Task PerformUpdateAsync(ReShade reShade)
    {
        throw new System.NotImplementedException();
    }

    public async Task PerformUpdateAsync(RenoDX renoDx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<bool> HasUpdateAvailableAsync(RenoDX installedRenoDx, RenoDXModPreference preference,
        RenoDXTagInfo modInfo)
    {
        throw new System.NotImplementedException();
    }
    
    public bool HasUpdate(ReShade reShade)
    {
        throw new System.NotImplementedException();
    }

    public bool HasUpdate(RenoDX renoDx)
    {
        throw new System.NotImplementedException();
    }
}