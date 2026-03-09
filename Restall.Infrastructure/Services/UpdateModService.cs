using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

public class UpdateModService : IUpdateModService
{
    private readonly ILogService _logService;

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