using Avalonia.Controls.ApplicationLifetimes;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.UI.DTOs;
using Restall.UI.Interfaces;
using Restall.UI.ViewModels.Dialogs;
using Restall.UI.Views.Dialogs;
using System.Threading.Tasks;

namespace Restall.UI.Services;

public class ModSelectionDialogService : IModSelectionDialogService
{
    private readonly ILogService _logService;
    private readonly IVersionCatalog _versionCatalog;

    public ModSelectionDialogService(
        ILogService logService,
        IVersionCatalog versionCatalog
        )
    {
        _logService = logService;
        _versionCatalog = versionCatalog;
    }

    public async Task<ReShadeInstallSelectionDto?> ShowReShadeInstallDialogAsync()
    {
        var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (mainWindow is null) return null;

        var versions = _versionCatalog.GetAvailableReShadeVersions(ReShade.Branch.Stable);

        if (versions.Count == 0)
        {
            await _logService.LogWarningAsync("No ReShade versions available.");
            return null;
        }

        var vm = new ReShadeInstallDialogViewModel(versions);
        var dialog = new ReShadeInstallDialog { DataContext = vm };
        
        await dialog.ShowDialog(mainWindow);
        
        return vm.WasConfirmed ? vm.BuildResult() : null;
    }
}