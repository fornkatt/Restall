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
using System.Threading;
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
    [NotifyPropertyChangedFor(nameof(RenoDXVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(RenoDXLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(ReShadeVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(RenoDXVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXUpdate))]
    [NotifyPropertyChangedFor(nameof(CanShowReShadeUpdate))]
    [NotifyPropertyChangedFor(nameof(RenoDXModStatus))]
    [NotifyPropertyChangedFor(nameof(RenoDXNotes))]
    [NotifyPropertyChangedFor(nameof(SpecificRenoDXModAvailableWarning))]
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

        OnPropertyChanged(nameof(RenoDXModActionStatus));
        OnPropertyChanged(nameof(CanShowReShadeUpdate));
        OnPropertyChanged(nameof(CanShowRenoDXUpdate));
        OnPropertyChanged(nameof(RenoDXVersionTextColor));
        OnPropertyChanged(nameof(ReShadeVersionTextColor));
        OnPropertyChanged(nameof(InstallReShadeButtonText));
        OnPropertyChanged(nameof(UpdateReShadeButtonText));
        OnPropertyChanged(nameof(UninstallReShadeButtonText));
        OnPropertyChanged(nameof(UpdateRenoDXButtonText));
        OnPropertyChanged(nameof(RenoDXNotes));
        OnPropertyChanged(nameof(SpecificRenoDXModAvailableWarning));
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
    
    [ObservableProperty]
    private string? _reShadeModActionStatus;
    
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
            ReShadeModActionStatus = report.PercentComplete >= 0
            ? $"Installing {report.Filename}... {report.PercentComplete}%"
            : $"Installing {report.Filename}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateReShadeAsync(request, progress);

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        ReShadeModActionStatus = result.IsSuccess ? successStatus : result.Message;
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
            ReShadeModActionStatus = report.PercentComplete >= 0
                ? $"Installing {report.Filename}... {report.PercentComplete}%"
                : $"Installing {report.Filename}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateReShadeAsync(request, progress);

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        ReShadeModActionStatus = result.IsSuccess ? successStatus : result.Message;
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
        ReShadeModActionStatus = result.IsSuccess ? "ReShade uninstalled." : result.Message;
    }

    private bool CanUninstallReShade => SelectedGame?.HasReShade ?? false;
    
    public bool CanShowReShadeUpdate =>
        SelectedGame?.HasReShade == true &&
        SelectedGame.ReShadeBranchName == SelectedReShadeBranch &&
        ReShadeLatestVersionForBranch is not null &&
        SelectedGame.ReShadeVersion is not null &&
        ReShadeLatestVersionForBranch != SelectedGame.ReShadeVersion;

    /* ---RENODX-------------------------------------------------------------------------------------------------------------- */
    
    [ObservableProperty]
    public string? _renoDXModActionStatus;

    [ObservableProperty]
    public bool _isShowingRenoDXActionMessage;
    
    private CancellationTokenSource? _renoDXMessageCts;

    private async Task ShowRenoDXActionMessageAsync(string message, int delayMs = 3500)
    {
        _renoDXMessageCts?.Cancel();
        _renoDXMessageCts = new CancellationTokenSource();
        
        var token = _renoDXMessageCts.Token;
        
        RenoDXModActionStatus = message;
        IsShowingRenoDXActionMessage = true;

        try
        {
            await Task.Delay(delayMs, token);
            RenoDXModActionStatus = null;
            IsShowingRenoDXActionMessage = false;
        }
        catch (OperationCanceledException) { }
    }
    
    public string? SpecificRenoDXModAvailableWarning =>
        SelectedGame?.IsUsingGenericModWhenSpecificAvailable == true
        ? """
        ⚡ A game-specific mod is now available for auto-install!
        
        Uninstall and reinstall to replace the generic mod.
        """
        : string.Empty;

    public string? RenoDXNotes
    {
        get
        {
            if (SelectedGame is null) return null;

            var mod = SelectedGame.CompatibleRenoDXMod;
            var genericMod = SelectedGame.CompatibleRenoDXGenericMod;
            var engine = SelectedGame.EngineName;

            if (mod is null && genericMod is null &&
                engine is Game.Engine.Unreal or Game.Engine.Unity)
            {
                return """
                    ❗ This game does not appear on the RenoDX wiki but downloads are allowed through the generic Unreal or Unity mods.

                    Compatibility is not guaranteed for these games.
                    """;
            }

            var text = RenoDXModStatus;
            var extraNotes = genericMod?.Notes;

            if (!string.IsNullOrWhiteSpace(mod?.Maintainer))
                text += $"""


                    Maintainer: {mod.Maintainer}
                    """;

            if (!string.IsNullOrWhiteSpace(extraNotes))
                text += $"""
                    

                    Additional notes:
                    
                    {extraNotes}
                    """;

            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
    }

    private string? RenoDXModStatus =>
        (SelectedGame?.CompatibleRenoDXMod?.Status ?? SelectedGame?.CompatibleRenoDXGenericMod?.Status) switch
        {
            ":white_check_mark:"    => "✅ Working",
            ":construction:"        => "🚧 WIP, may lack testing or have deal-breaking issues",
            _                       => string.Empty
        };

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

    private bool CanInstallRenoDX => SelectedGame is not null                               &&
                                    (SelectedGame.CompatibleRenoDXMod is not null           ||
                                     SelectedGame.CompatibleRenoDXGenericMod is not null    ||
                                     SelectedGame.EngineName == Game.Engine.Unity           ||
                                     SelectedGame.EngineName == Game.Engine.Unreal          ||
                                     SelectedGame.HasRenoDX)                                &&
                                     SelectedGame.HasReShade;

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
            RenoDXModActionStatus = report.PercentComplete >= 0
                ? $"Installing {report.Filename}... {report.PercentComplete}%"
                : $"Installing {report.Filename}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateRenoDXAsync(request, progress);

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        _ = ShowRenoDXActionMessageAsync(result.IsSuccess ? successStatus : result.Message!, 5000);
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
            RenoDXModActionStatus = report.PercentComplete >= 0
                ? $"Installing {report.Filename}... {report.PercentComplete}%"
                : $"Installing {report.Filename}...";
        });

        var result = await _modManagementFacade.InstallOrUpdateRenoDXAsync(request, progress);

        SelectedGame.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        await ShowRenoDXActionMessageAsync(result.IsSuccess ? successStatus : result.Message!);
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
        await ShowRenoDXActionMessageAsync(result.IsSuccess ? "RenoDX uninstalled." : result.Message!);
    }

    private bool CanUninstallRenoDX => SelectedGame?.HasRenoDX ?? false;

    public bool CanShowRenoDXUpdate =>
        SelectedGame?.HasRenoDX == true                             &&
        SelectedGame.EngineName != Game.Engine.Unity                &&
        SelectedGame.RenoDXBranchName == SelectedRenoDXBranch       &&
        RenoDXLatestVersionForBranch is not null                    &&
        SelectedGame.RenoDXVersion is not null                      &&
        RenoDXLatestVersionForBranch != SelectedGame.RenoDXVersion;
}