using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Application.UseCases;
using Restall.Domain.Entities;
using Restall.UI.Interfaces;
using Restall.UI.Messages;
using System;
using System.Threading.Tasks;

namespace Restall.UI.ViewModels;

public partial class ModViewModel : ViewModelBase, IRecipient<SelectedGameChangedMessage>
{
    private readonly IModManagementFacade _modManagementFacade;
    private readonly IModSelectionDialogService _modSelectionDialogService;
    private readonly IParseService _parseService;

    private bool _suppressMessage;

    public ModViewModel(
    IModManagementFacade modManagementFacade,
    IModSelectionDialogService modSelectionDialogService,
    IParseService parseService
    )
    {
        _modManagementFacade = modManagementFacade;
        _modSelectionDialogService = modSelectionDialogService;
        _parseService = parseService;

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
    private GameModViewModel? _selectedGame;

    [ObservableProperty]
    private ReShade.Branch _selectedReShadeBranch = ReShade.Branch.Stable;

    public string? ReShadeLatestVersionForBranch =>
        _parseService.GetLatestReShadeVersion(SelectedReShadeBranch);

    [ObservableProperty]
    private RenoDX.Branch _selectedRenoDXBranch = RenoDX.Branch.Snapshot;

    public string? RenoDXLatestVersionForBranch => SelectedGame?.EngineName == Game.Engine.Unity
        ? "No version info"
        : _parseService.GetLatestRenoDXTag(SelectedRenoDXBranch)?.Version;


    [ObservableProperty]
    private int _downloadPercent;

    [ObservableProperty]
    private string? _downloadStatus;

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

        OnPropertyChanged(nameof(InstallReShadeButtonText));
        OnPropertyChanged(nameof(UpdateReShadeButtonText));
        OnPropertyChanged(nameof(UninstallReShadeButtonText));
        OnPropertyChanged(nameof(UpdateRenoDXButtonText));
    }

    /* ---RESHADE-------------------------------------------------------------------------------------------------------------- */

    public string InstallReShadeButtonText =>
        SelectedGame?.HasReShade == true ? "Reinstall" : "Install";

    public string UpdateReShadeButtonText => "Update";

    public string UninstallReShadeButtonText => "Uninstall";

    [RelayCommand(CanExecute = nameof(CanInstallReShade))]
    private Task InstallReShadeAsync() => ExecuteReShadeInstallAsync("ReShade installed.");

    private bool CanInstallReShade => SelectedGame is not null;

    [RelayCommand(CanExecute = nameof(CanUpdateReShade))]
    private Task UpdateReShadeAsync() => ExecuteReShadeInstallAsync("ReShade updated.");

    private bool CanUpdateReShade => SelectedGame?.CanUpdateReShade ?? false;

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
            DownloadStatus = report.PercentComplete >= 0
            ? $"Downloading {report.FileName}... {report.PercentComplete}%"
            : $"Downloading {report.FileName}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateReShadeAsync(request, progress);

        if (result.UpdateCheckResult is not null)
            SelectedGame.ReShadeUpdateResult = result.UpdateCheckResult;

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        DownloadStatus = result.IsSuccess ? successStatus : result.ErrorMessage;
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

    /* ---RENODX-------------------------------------------------------------------------------------------------------------- */

    public string InstallRenoDXButtonText =>
        SelectedGame?.HasRenoDX == true ? "Reinstall" : "Install";

    public string UpdateRenoDXButtonText => "Update";

    public string UninstallRenoDXButtonText => "Uninstall";

    [RelayCommand(CanExecute = nameof(CanInstallRenoDX))]
    private Task InstallRenoDXAsync() => ExecuteRenoDXInstallAsync("RenoDX installed.");

    private bool CanInstallRenoDX => SelectedGame?.CanInstallRenoDX ?? false;

    [RelayCommand(CanExecute = nameof(CanUpdateRenoDX))]
    private Task UpdateRenoDXAsync() => ExecuteRenoDXInstallAsync("RenoDX updated.");

    private bool CanUpdateRenoDX => SelectedGame?.CanUpdateRenoDX ?? false;

    private async Task ExecuteRenoDXInstallAsync(string successStatus)
    {
        var request = new InstallRenoDXRequest(
            SelectedGame!.GetGame(),
            SelectedGame.SelectedRenoDXInstallArch,
            SelectedRenoDXBranch,
            ModInfo: SelectedGame.CompatibleRenoDXMod,
            GenericModInfo: SelectedGame.CompatibleRenoDXGenericMod,
            TargetVersion: SelectedGame.RenoDXLatestVersion
            );

        var progress = new Progress<DownloadProgressReportDto>(report =>
        {
            DownloadPercent = report.PercentComplete;
            DownloadStatus = report.PercentComplete >= 0
                ? $"Downloading {report.FileName}... {report.PercentComplete}%"
                : $"Downloading {report.FileName}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateRenoDXAsync(request, progress);

        if (result.UpdateCheckResult is not null)
            SelectedGame.RenoDXUpdateResult = result.UpdateCheckResult;

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        DownloadStatus = result.IsSuccess ? successStatus : result.ErrorMessage;
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
}