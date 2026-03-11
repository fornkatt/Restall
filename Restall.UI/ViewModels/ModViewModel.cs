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

    private bool _suppressMessage;

    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    [ObservableProperty]
    private ReShade.Branch _selectedReShadeBranch = ReShade.Branch.Stable;

    [ObservableProperty]
    private RenoDX.Branch _selectedRenoDXBranch = RenoDX.Branch.Snapshot;

    [ObservableProperty]
    private int _downloadPercent;

    [ObservableProperty]
    private string? _downloadStatus;

    [ObservableProperty]
    private string? _uninstallStatus;

    public ModViewModel(
        IModManagementFacade modManagementFacade,
        IModSelectionDialogService modSelectionDialogService
        )
    {
        _modManagementFacade = modManagementFacade;
        _modSelectionDialogService = modSelectionDialogService;

        IsActive = true;
    }

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
    }

    /* ---RESHADE-------------------------------------------------------------------------------------------------------------- */

    [RelayCommand(CanExecute = nameof(CanInstallReShade))]
    private Task InstallReShadeAsync() => ExecuteReShadeInstallAsync("ReShade installed.");

    private bool CanInstallReShade => SelectedGame?.CanInstallReShade ?? false;

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

    [RelayCommand(CanExecute = nameof(CanInstallRenoDX))]
    private Task InstallRenoDXAsync() => ExecuteRenoDXInstallAsync("RenoDX installed.");

    private bool CanInstallRenoDX => SelectedGame?.CanInstallRenoDX ?? false;

    [RelayCommand(CanExecute = nameof(CanUpdateRenoDX))]
    private Task UpdateRenoDXAsync() => ExecuteRenoDXInstallAsync("RenoDX updated.");

    private bool CanUpdateRenoDX => SelectedGame?.CanUpdateRenoDX ?? false;

    private async Task ExecuteRenoDXInstallAsync(string successStatus)
    {
        if (SelectedGame!.CompatibleRenoDXMod is null && SelectedGame.CompatibleRenoDXGenericMod is null) return;

        var request = new InstallRenoDXRequest(
            SelectedGame.GetGame(),
            SelectedGame.SelectedRenoDXInstallArch,
            SelectedRenoDXBranch,
            ModInfo: SelectedGame.CompatibleRenoDXMod,
            GenericModInfo: SelectedGame.CompatibleRenoDXGenericMod
            );

        var progress = new Progress<DownloadProgressReportDto>(report =>
        {
            DownloadPercent = report.PercentComplete;
            DownloadStatus = report.PercentComplete >= 0
                ? $"Downloading {report.FileName}... {report.PercentComplete}%"
                : $"Downloading {report.FileName}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateRenoDXAsync(request, progress);

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