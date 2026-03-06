using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

public class UpdateModService : IUpdateModService
{
    private readonly ILogService _logService;

    private const string ReShadeStartUrl = "https://reshade.me/downloads/ReShade_Setup_";
    private const string ReShadeEndUrl = "_Addon.exe";
    private const string RenoDxUrl = "https://github.com/clshortfuse/renodx/wiki/Mods/";
    private const string RenoDxTagUrl = "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd

    public UpdateModService(ILogService logService)
    {
        _logService = logService;
    }

    public async Task PerformUpdateAsync(ReShade reShade)
    {
        throw new System.NotImplementedException();
    }

    public async Task PerformUpdateAsync(RenoDX renoDx)
    {
        throw new System.NotImplementedException();
    }

    public async Task<bool> HasUpdateAvailableAsync(RenoDX installedRenoDx, RenoDXModPreferenceDto preferenceDto,
        RenoDXTagInfoDto modInfoDto)
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