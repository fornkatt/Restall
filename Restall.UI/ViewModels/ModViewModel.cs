using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.UI.Interfaces;
using Restall.UI.Messages;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Restall.Application.UseCases.Requests;

namespace Restall.UI.ViewModels;

public partial class ModViewModel : ViewModelBase, IRecipient<SelectedGameChangedMessage>
{
    private readonly IModManagementFacade _modManagementFacade;
    private readonly IModSelectionDialogService _modSelectionDialogService;
    private readonly IVersionCatalog _versionCatalog;

    private bool _suppressMessage;

    public ModViewModel(
    IModManagementFacade modManagementFacade,
    IModSelectionDialogService modSelectionDialogService,
    IVersionCatalog versionCatalog
    )
    {
        _modManagementFacade = modManagementFacade;
        _modSelectionDialogService = modSelectionDialogService;
        _versionCatalog = versionCatalog;

        IsActive = true;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InstallReShadeButtonText))]
    [NotifyPropertyChangedFor(nameof(UpdateReShadeButtonText))]
    [NotifyPropertyChangedFor(nameof(UninstallReShadeButtonText))]
    [NotifyPropertyChangedFor(nameof(InstallRenoDXButtonText))]
    [NotifyPropertyChangedFor(nameof(UpdateRenoDXButtonText))]
    [NotifyPropertyChangedFor(nameof(UninstallRenoDXButtonText))]
    [NotifyPropertyChangedFor(nameof(RenoDXLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(ReShadeVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(RenoDXVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXUpdate))]
    [NotifyPropertyChangedFor(nameof(CanShowReShadeUpdate))]
    private GameModViewModel? _selectedGame;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReShadeLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(ReShadeVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowReShadeUpdate))]
    private ReShade.Branch _selectedReShadeBranch = ReShade.Branch.Stable;

    public string? ReShadeLatestVersionForBranch =>
        _versionCatalog.GetLatestReShadeVersion(SelectedReShadeBranch);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RenoDXLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(IsRenoDXNightlyBranch))]
    [NotifyPropertyChangedFor(nameof(RenoDXVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXUpdate))]
    private RenoDX.Branch _selectedRenoDXBranch = RenoDX.Branch.Snapshot;

    public bool IsRenoDXNightlyBranch
    {
        get => SelectedRenoDXBranch == RenoDX.Branch.Nightly;
        set => SelectedRenoDXBranch = value ? RenoDX.Branch.Nightly : RenoDX.Branch.Snapshot;
    }

    public string? RenoDXLatestVersionForBranch => SelectedGame?.EngineName == Game.Engine.Unity
        ? "No version info"
        : _versionCatalog.GetLatestRenoDXVersionByTag(SelectedRenoDXBranch)?.Version;


    [ObservableProperty]
    private int _downloadPercent;

    [ObservableProperty]
    private string? _installStatus;

    [ObservableProperty]
    private string? _updateStatus;

    [ObservableProperty]
    private string? _uninstallStatus;

    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        if (!_suppressMessage)
            Messenger.Send(new SelectedGameChangedMessage(value));

        NotifyAllCommandsChanged();
    }

    public void Receive(SelectedGameChangedMessage message)
    {
        _suppressMessage = true;
        SelectedGame = message.Value;
        _suppressMessage = false;
    }

    private void NotifyAllCommandsChanged()
    {
        InstallReShadeCommand.NotifyCanExecuteChanged();
        UpdateReShadeCommand.NotifyCanExecuteChanged();
        UninstallReShadeCommand.NotifyCanExecuteChanged();
        InstallRenoDXCommand.NotifyCanExecuteChanged();
        UpdateRenoDXCommand.NotifyCanExecuteChanged();
        UninstallRenoDXCommand.NotifyCanExecuteChanged();

        OnPropertyChanged(nameof(CanShowReShadeUpdate));
        OnPropertyChanged(nameof(CanShowRenoDXUpdate));
        OnPropertyChanged(nameof(RenoDXVersionTextColor));
        OnPropertyChanged(nameof(ReShadeVersionTextColor));
        OnPropertyChanged(nameof(InstallReShadeButtonText));
        OnPropertyChanged(nameof(UpdateReShadeButtonText));
        OnPropertyChanged(nameof(UninstallReShadeButtonText));
        OnPropertyChanged(nameof(UpdateRenoDXButtonText));
    }
    
    /* ---GAME CARD-------------------------------------------------------------------------------------------------------------- */
    [RelayCommand]
    private void OpenInExplorer()
    {
        var folder = SelectedGame?.ExecutablePath;
        if(!Directory.Exists(folder)) return;
        
        if (OperatingSystem.IsWindows())
        {
            
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{folder}\"",
                UseShellExecute = false
            });
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open",
                ArgumentList = { folder },
                UseShellExecute = false
            });
        }
    }
    
    /* ---RESHADE-------------------------------------------------------------------------------------------------------------- */

    public string? ReShadeVersionTextColor =>
        SelectedGame?.HasReShade == true ? 
            (CanShowReShadeUpdate ? "#eb5a2f" : "#1ab652")
            : null;
    
    public string InstallReShadeButtonText =>
        SelectedGame?.HasReShade == true ? "Reinstall" : "Install";

    public string UpdateReShadeButtonText => "Update";

    public string UninstallReShadeButtonText => "Uninstall";

    [RelayCommand(CanExecute = nameof(CanInstallReShade))]
    private Task InstallReShadeAsync() => ExecuteReShadeInstallAsync("ReShade installed.");

    private bool CanInstallReShade => SelectedGame is not null;

    [RelayCommand(CanExecute = nameof(CanUpdateReShade))]
    private Task UpdateReShadeAsync() => ExecuteReShadeUpdateAsync("ReShade updated.");

    private bool CanUpdateReShade => CanShowReShadeUpdate;

    private async Task ExecuteReShadeInstallAsync(string successStatus)
    {
        var selection = await _modSelectionDialogService.ShowReShadeInstallDialogAsync();
        if (selection is null) return;

        var request = new InstallReShadeRequest(
            SelectedGame!.GetGame(),
            SelectedReShadeBranch,
            SelectedGame.SelectedReShadeInstallArch,
            selection.Version,
            ReShade.GetFileName(selection.FileName, selection.FileExtension)
            );

        var progress = new Progress<DownloadProgressReportDto>(report =>
        {
            DownloadPercent = report.PercentComplete;
            InstallStatus = report.PercentComplete >= 0
            ? $"Downloading {report.FileName}... {report.PercentComplete}%"
            : $"Downloading {report.FileName}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateReShadeAsync(request, progress);

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        InstallStatus = result.IsSuccess ? successStatus : result.ErrorMessage;
    }

    private async Task ExecuteReShadeUpdateAsync(string successStatus)
    {
        var installedFilename = SelectedGame?.ReShadeFilename;
        var latestVersion = _versionCatalog.GetLatestReShadeVersion(SelectedReShadeBranch);

        if (installedFilename is null || latestVersion is null) return;

        var request = new InstallReShadeRequest(
            SelectedGame!.GetGame(),
            SelectedReShadeBranch,
            SelectedGame.SelectedReShadeInstallArch,
            latestVersion,
            installedFilename
            );

        var progress = new Progress<DownloadProgressReportDto>(report =>
        {
            DownloadPercent = report.PercentComplete;
            InstallStatus = report.PercentComplete >= 0
                ? $"Downloading {report.FileName}... {report.PercentComplete}%"
                : $"Downloading {report.FileName}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateReShadeAsync(request, progress);

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        InstallStatus = result.IsSuccess ? successStatus : result.ErrorMessage;
    }

    [RelayCommand(CanExecute = nameof(CanUninstallReShade))]
    private async Task UninstallReShadeAsync()
    {
        var result = await _modManagementFacade.UninstallReShadeAsync(SelectedGame!.GetGame());

        if (result.ShouldPromptForDeepScan)
        {
            // TODO Implement deep scan confirmation dialog
        }

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        UninstallStatus = result.IsSuccess ? "ReShade uninstalled." : result.ErrorMessage;
    }

    private bool CanUninstallReShade => SelectedGame?.HasReShade ?? false;
    
    public bool CanShowReShadeUpdate =>
        SelectedGame?.HasReShade == true &&
        SelectedGame.ReShadeBranchName == SelectedReShadeBranch &&
        ReShadeLatestVersionForBranch is not null &&
        SelectedGame.ReShadeVersion is not null &&
        ReShadeLatestVersionForBranch != SelectedGame.ReShadeVersion;

    /* ---RENODX-------------------------------------------------------------------------------------------------------------- */

    public string? RenoDXVersionTextColor =>
        SelectedGame?.HasRenoDX == true
            ? (CanShowRenoDXUpdate ? "#eb5a2f" : "#1ab652")
            : null;
    
    public string InstallRenoDXButtonText =>
        SelectedGame?.HasRenoDX == true ? "Reinstall" : "Install";

    public string UpdateRenoDXButtonText => "Update";

    public string UninstallRenoDXButtonText => "Uninstall";

    [RelayCommand(CanExecute = nameof(CanInstallRenoDX))]
    private Task InstallRenoDXAsync() => ExecuteRenoDXInstallAsync("RenoDX installed.");

    private bool CanInstallRenoDX => SelectedGame?.CanInstallRenoDX ?? false;

    [RelayCommand(CanExecute = nameof(CanUpdateRenoDX))]
    private Task UpdateRenoDXAsync() => ExecuteRenoDXUpdateAsync("RenoDX updated.");

    private bool CanUpdateRenoDX => CanShowRenoDXUpdate;

    private async Task ExecuteRenoDXInstallAsync(string successStatus)
    {
        string? targetVersion;

        if (SelectedRenoDXBranch == RenoDX.Branch.Nightly)
        {
            var selectedTag = await _modSelectionDialogService.ShowRenoDXInstallDialogAsync();
            if (selectedTag is null) return;

            targetVersion = selectedTag.Version;
        }
        else
        {
            targetVersion = RenoDXLatestVersionForBranch;
        }
            
        var request = new InstallRenoDXRequest(
            SelectedGame!.GetGame(),
            SelectedGame.SelectedRenoDXInstallArch,
            SelectedRenoDXBranch,
            ModInfo: SelectedGame.CompatibleRenoDXMod,
            GenericModInfo: SelectedGame.CompatibleRenoDXGenericMod,
            TargetVersion: targetVersion
            );

        var progress = new Progress<DownloadProgressReportDto>(report =>
        {
            DownloadPercent = report.PercentComplete;
            InstallStatus = report.PercentComplete >= 0
                ? $"Downloading {report.FileName}... {report.PercentComplete}%"
                : $"Downloading {report.FileName}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateRenoDXAsync(request, progress);

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        InstallStatus = result.IsSuccess ? successStatus : result.ErrorMessage;
    }

    private async Task ExecuteRenoDXUpdateAsync(string successStatus)
    {
        var targetVersion = RenoDXLatestVersionForBranch;
        if (targetVersion is null) return;
        
        var request = new InstallRenoDXRequest(
            SelectedGame!.GetGame(),
            SelectedGame.SelectedRenoDXInstallArch,
            SelectedRenoDXBranch,
            ModInfo: SelectedGame.CompatibleRenoDXMod,
            GenericModInfo: SelectedGame.CompatibleRenoDXGenericMod,
            TargetVersion: targetVersion
        );

        var progress = new Progress<DownloadProgressReportDto>(report =>
        {
            DownloadPercent = report.PercentComplete;
            InstallStatus = report.PercentComplete >= 0
                ? $"Downloading {report.FileName}... {report.PercentComplete}%"
                : $"Downloading {report.FileName}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateRenoDXAsync(request, progress);

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        InstallStatus = result.IsSuccess ? successStatus : result.ErrorMessage;
    }

    [RelayCommand(CanExecute = nameof(CanUninstallRenoDX))]
    private async Task UninstallRenoDXAsync()
    {
        var result = await _modManagementFacade.UninstallRenoDXAsync(SelectedGame!.GetGame());

        if (result.ShouldPromptForDeepScan)
        {
            // TODO Implement deep scan confirmation dialog
        }

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        UninstallStatus = result.IsSuccess ? "RenoDX uninstalled." : result.ErrorMessage;
    }

    private bool CanUninstallRenoDX => SelectedGame?.HasRenoDX ?? false;

    public bool CanShowRenoDXUpdate =>
        SelectedGame?.HasRenoDX == true &&
        SelectedGame.EngineName != Game.Engine.Unity &&
        SelectedGame.RenoDXBranchName == SelectedRenoDXBranch &&
        RenoDXLatestVersionForBranch is not null &&
        SelectedGame.RenoDXVersion is not null &&
        RenoDXLatestVersionForBranch != SelectedGame.RenoDXVersion;
}