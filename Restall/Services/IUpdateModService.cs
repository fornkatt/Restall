using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public interface IUpdateModService
{
    Task PerformUpdateAsync(ReShade reShade);
    Task PerformUpdateAsync(RenoDX renoDx);
    Task<bool> HasUpdateAvailableAsync(RenoDX installedRenoDx, RenoDXModPreference preference,
        RenoDXTagInfo modInfo);

    bool HasUpdate(ReShade reShade);
    bool HasUpdate(RenoDX renoDx);
}