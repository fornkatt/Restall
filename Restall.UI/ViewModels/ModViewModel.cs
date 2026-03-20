using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;
using Restall.UI.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Restall.UI.ViewModels;

public sealed partial class ModViewModel : ViewModelBase
{
    private readonly IModManagementFacade _modManagementFacade;
    private readonly IModSelectionDialogService _modSelectionDialogService;
    private readonly IVersionCatalog _versionCatalog;

    public ModViewModel(
        IModManagementFacade modManagementFacade,
        IModSelectionDialogService modSelectionDialogService,
        IVersionCatalog versionCatalog
    )
    {
        _modManagementFacade = modManagementFacade;
        _modSelectionDialogService = modSelectionDialogService;
        _versionCatalog = versionCatalog;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InstallReShadeButtonText))]
    [NotifyPropertyChangedFor(nameof(UpdateReShadeButtonText))]
    [NotifyPropertyChangedFor(nameof(UninstallReShadeButtonText))]
    [NotifyPropertyChangedFor(nameof(InstallRenoDXButtonText))]
    [NotifyPropertyChangedFor(nameof(UpdateRenoDXButtonText))]
    [NotifyPropertyChangedFor(nameof(UninstallRenoDXButtonText))]
    [NotifyPropertyChangedFor(nameof(RenoDXVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(ReShadeVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXUpdate))]
    [NotifyPropertyChangedFor(nameof(CanShowReShadeUpdate))]
    [NotifyPropertyChangedFor(nameof(RenoDXModStatus))]
    [NotifyPropertyChangedFor(nameof(RenoDXNotes))]
    [NotifyPropertyChangedFor(nameof(SpecificRenoDXModAvailableWarning))]
    [NotifyPropertyChangedFor(nameof(IsNightlyBranchAvailable))]
    private GameModViewModel? _selectedGame;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReShadeLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(ReShadeVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowReShadeUpdate))]
    [NotifyCanExecuteChangedFor(nameof(UpdateReShadeCommand))]
    private ReShade.Branch _selectedReShadeBranch = ReShade.Branch.Stable;

    public string? ReShadeLatestVersionForBranch =>
        _versionCatalog.GetLatestReShadeVersion(SelectedReShadeBranch);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RenoDXLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(IsRenoDXNightlyBranch))]
    [NotifyPropertyChangedFor(nameof(RenoDXVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXUpdate))]
    [NotifyCanExecuteChangedFor(nameof(UpdateRenoDXCommand))]
    private RenoDX.Branch _selectedRenoDXBranch = RenoDX.Branch.Snapshot;

    public bool IsNightlyBranchAvailable => 
        SelectedGame is not null &&
        !(SelectedGame.EngineName == Game.Engine.Unity && SelectedGame.CompatibleRenoDXMod is null);

    public bool IsRenoDXNightlyBranch
    {
        get => SelectedRenoDXBranch == RenoDX.Branch.Nightly;
        set => SelectedRenoDXBranch = value ? RenoDX.Branch.Nightly : RenoDX.Branch.Snapshot;
    }

    public string? RenoDXLatestVersionForBranch =>
        _versionCatalog.GetLatestRenoDXVersionByTag(SelectedRenoDXBranch)?.Version;

    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        if (value?.EngineName == Game.Engine.Unity && value.CompatibleRenoDXMod is null)
            SelectedRenoDXBranch = RenoDX.Branch.Snapshot;

        NotifyAllCommandsChanged();
    }

    public void ApplyWikiRefresh() => NotifyAllCommandsChanged();

    public void ApplySelectedGame(GameModViewModel? value) => SelectedGame = value;

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
        OnPropertyChanged(nameof(RenoDXNotes));
        OnPropertyChanged(nameof(SpecificRenoDXModAvailableWarning));
    }

    /* ---GAME CARD-------------------------------------------------------------------------------------------------------------- */
    [RelayCommand]
    private void OpenInExplorer()
    {
        var folder = SelectedGame?.ExecutablePath;
        if (!Directory.Exists(folder)) return;

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
    private async Task ExecuteReShadeActionAsync(Func<Progress<DownloadProgressReportDto>,
        Task<ModOperationResultDto>> work, string successMessage, int delayMs = 5000)
    {
        var game = SelectedGame!;

        game._reShadeMessageCts?.Cancel();
        var cts = new CancellationTokenSource();
        game._reShadeMessageCts = cts;

        var progress = new Progress<DownloadProgressReportDto>(report =>
        {
            game.ReShadeModActionStatus = report.PercentComplete >= 0
                ? $"""
                   Downloading {report.Filename}
                   {report.PercentComplete}%
                   """
                : $"Downloading {report.Filename}";
            game.IsShowingReShadeActionMessage = true;
        });

        var result = await work(progress);

        game.ReShadeUpdateCheck = result.UpdateCheckResult;
        game.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        game.ReShadeModActionStatus = result.IsSuccess ? successMessage : result.Message;
        game.IsShowingReShadeActionMessage = true;

        _ = DismissAsync();

        async Task DismissAsync()
        {
            try { await Task.Delay(delayMs, cts.Token); }
            catch (OperationCanceledException) { }
            game.ReShadeModActionStatus = null;
            game.IsShowingReShadeActionMessage = false;
        }
    }

    public string? ReShadeVersionTextColor =>
        SelectedGame?.HasReShade == true ?
            (CanShowReShadeUpdate ? "#eb5a2f" : "#1ab652")
            : null;

    public string InstallReShadeButtonText =>
        SelectedGame?.HasReShade == true ? "Reinstall" : "Install";

    public string UpdateReShadeButtonText => "Update";

    public string UninstallReShadeButtonText => "Uninstall";

    [RelayCommand(CanExecute = nameof(CanInstallReShade))]
    private async Task InstallReShadeAsync()
    {
        var selection = await _modSelectionDialogService.ShowReShadeInstallDialogAsync();
        if (selection is null) return;

        var request = new InstallReShadeRequest(
            SelectedGame!.GetGame(),
            SelectedReShadeBranch,
            SelectedGame.SelectedReShadeInstallArch,
            selection.Version,
            ReShade.GetFileName(selection.Filename, selection.FileExtension)
        );

        await ExecuteReShadeActionAsync(
            p => _modManagementFacade.InstallOrUpdateReShadeAsync(request, p),
            "ReShade installed.");
    }

    private bool CanInstallReShade => SelectedGame is not null;

    [RelayCommand(CanExecute = nameof(CanUpdateReShade))]
    private async Task UpdateReShadeAsync()
    {
        var installedFilename = SelectedGame?.ReShadeFilename;
        var latestVersion = ReShadeLatestVersionForBranch;

        if (installedFilename is null || latestVersion is null) return;

        var request = new InstallReShadeRequest(
            SelectedGame!.GetGame(),
            SelectedReShadeBranch,
            SelectedGame.SelectedReShadeInstallArch,
            latestVersion,
            installedFilename
        );

        await ExecuteReShadeActionAsync(
            p => _modManagementFacade.InstallOrUpdateReShadeAsync(request, p),
            "ReShade updated.");
    }

    private bool CanUpdateReShade => CanShowReShadeUpdate;

    [RelayCommand(CanExecute = nameof(CanUninstallReShade))]
    private Task UninstallReShadeAsync() =>
        ExecuteReShadeActionAsync(_ => _modManagementFacade.UninstallReShadeAsync(SelectedGame!.GetGame()),
            "ReShade uninstalled.");

    private bool CanUninstallReShade => SelectedGame?.HasReShade ?? false;

    public bool CanShowReShadeUpdate =>
        SelectedGame?.HasReShade == true                            &&
        SelectedGame.ReShadeBranchName == SelectedReShadeBranch     &&
        SelectedGame.ReShadeUpdateCheck?.UpdateAvailable == true;

    /* ---RENODX-------------------------------------------------------------------------------------------------------------- */
    private async Task ExecuteRenoDXActionAsync(
        Func<Progress<DownloadProgressReportDto>, Task<ModOperationResultDto>> work,
        string successMessage,
        int delayMs = 5000)
    {
        var game = SelectedGame!;

        game._renoDXMessageCts?.Cancel();
        var cts = new CancellationTokenSource();
        game._renoDXMessageCts = cts;

        var progress = new Progress<DownloadProgressReportDto>(report =>
        {
            game.RenoDXModActionStatus = report.PercentComplete >= 0
                ? $"""
                   Downloading {report.Filename}
                   {report.PercentComplete}%
                   """
                : $"Downloading {report.Filename}";
            game.IsShowingRenoDXActionMessage = true;
        });

        var result = await work(progress);

        game.RenoDXUpdateCheck = result.UpdateCheckResult;
        game.NotifyGameStateChanged();
        NotifyAllCommandsChanged();
        game.RenoDXModActionStatus = result.IsSuccess ? successMessage : result.Message;
        game.IsShowingRenoDXActionMessage = true;

        _ = DismissAsync();

        async Task DismissAsync()
        {
            try { await Task.Delay(delayMs, cts.Token); }
            catch (OperationCanceledException) { return; }
            game.RenoDXModActionStatus = null;
            game.IsShowingRenoDXActionMessage = false;
        }
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
            ":white_check_mark:" => "✅ Working",
            ":construction:" => "🚧 WIP, may lack testing or have deal-breaking issues",
            _ => string.Empty
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
    private async Task InstallRenoDXAsync()
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

        await ExecuteRenoDXActionAsync(
            p => _modManagementFacade.InstallOrUpdateRenoDXAsync(request, p),
            "RenoDX installed.");
    }

    private bool CanInstallRenoDX => SelectedGame is not null &&
                                    (SelectedGame.CompatibleRenoDXMod is not null ||
                                     SelectedGame.CompatibleRenoDXGenericMod is not null ||
                                     SelectedGame.EngineName == Game.Engine.Unity ||
                                     SelectedGame.EngineName == Game.Engine.Unreal ||
                                     SelectedGame.HasRenoDX) &&
                                     SelectedGame.HasReShade;

    [RelayCommand(CanExecute = nameof(CanUpdateRenoDX))]
    private async Task UpdateRenoDXAsync()
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

        await ExecuteRenoDXActionAsync(
            p => _modManagementFacade.InstallOrUpdateRenoDXAsync(request, p),
            "RenoDX updated.");
    }

    private bool CanUpdateRenoDX => CanShowRenoDXUpdate;

    [RelayCommand(CanExecute = nameof(CanUninstallRenoDX))]
    private Task UninstallRenoDXAsync() =>
    ExecuteRenoDXActionAsync(
        _ => _modManagementFacade.UninstallRenoDXAsync(SelectedGame!.GetGame()),
        "RenoDX uninstalled.");

    private bool CanUninstallRenoDX => SelectedGame?.HasRenoDX ?? false;

    public bool CanShowRenoDXUpdate =>
        SelectedGame?.HasRenoDX == true                         &&
        SelectedGame.EngineName != Game.Engine.Unity            &&
        SelectedGame.RenoDXBranchName == SelectedRenoDXBranch   &&
        SelectedGame.RenoDXUpdateCheck?.UpdateAvailable == true;
}