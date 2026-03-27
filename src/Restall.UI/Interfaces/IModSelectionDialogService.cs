using Restall.Application.DTOs;
using Restall.UI.DTOs;
using System.Threading.Tasks;

namespace Restall.UI.Interfaces;

public interface IModSelectionDialogService
{
    Task<ReShadeInstallSelectionDto?> ShowReShadeInstallDialogAsync();
    Task<RenoDXTagInfoDto?> ShowRenoDXInstallDialogAsync();
}