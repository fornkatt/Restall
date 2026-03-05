using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IUpdateModService
{
    Task PerformUpdateAsync(ReShade reShade);
    Task PerformUpdateAsync(RenoDX renoDx);
    Task<bool> HasUpdateAvailableAsync(RenoDX installedRenoDx, RenoDXModPreferenceDto preferenceDto,
        RenoDXTagInfoDto modInfoDto);

    bool HasUpdate(ReShade reShade);
    bool HasUpdate(RenoDX renoDx);
}