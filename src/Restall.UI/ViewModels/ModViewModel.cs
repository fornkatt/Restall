using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;
using Restall.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Restall.Application.DTOs.Results;

namespace Restall.UI.ViewModels;

public sealed partial class ModViewModel : ViewModelBase
{
    private readonly IModManagementFacade _modManagementFacade;
    private readonly IModSelectionDialogService _modSelectionDialogService;
    private readonly IVersionCatalog _versionCatalog;

    private const string s_upToDateTextColor = "#eb5a2f";
    private const string s_updateAvailableTextColor = "#1ab652";

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
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXBranchSelector))]
    [NotifyPropertyChangedFor(nameof(AvailableRenoDXBranches))]
    private GameModViewModel? _selectedGame;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReShadeLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(ReShadeVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowReShadeUpdate))]
    [NotifyCanExecuteChangedFor(nameof(UpdateReShadeCommand))]
    private ReShade.Branch _selectedReShadeBranch = ReShade.Branch.Stable;

    public string? ReShadeLatestVersionForBranch =>
        _versionCatalog.GetLatestReShadeVersion(SelectedReShadeBranch);

    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        RefreshAvailableRenoDXBranches(value);
        NotifyAllCommandsChanged();
    }

    public void ApplyWikiRefresh()
    {
        RefreshAvailableRenoDXBranches(SelectedGame);
        NotifyAllCommandsChanged();
    }

    public void ApplySelectedGame(GameModViewModel? value) => SelectedGame = value;

    private void NotifyAllCommandsChanged()
    {
        InstallReShadeCommand.NotifyCanExecuteChanged();
        UpdateReShadeCommand.NotifyCanExecuteChanged();
        UninstallReShadeCommand.NotifyCanExecuteChanged();
        UpdateRenoDXCommand.NotifyCanExecuteChanged();
        UninstallRenoDXCommand.NotifyCanExecuteChanged();
        RenoDXInstallButtonClickCommand.NotifyCanExecuteChanged();

        OnPropertyChanged(nameof(CanShowReShadeUpdate));
        OnPropertyChanged(nameof(CanShowRenoDXUpdate));
        OnPropertyChanged(nameof(RenoDXVersionTextColor));
        OnPropertyChanged(nameof(ReShadeVersionTextColor));
        OnPropertyChanged(nameof(InstallReShadeButtonText));
        OnPropertyChanged(nameof(UpdateReShadeButtonText));
        OnPropertyChanged(nameof(UninstallReShadeButtonText));
        OnPropertyChanged(nameof(InstallRenoDXButtonText));
        OnPropertyChanged(nameof(UninstallRenoDXButtonText));
        OnPropertyChanged(nameof(UpdateRenoDXButtonText));
        OnPropertyChanged(nameof(RenoDXNotes));
        OnPropertyChanged(nameof(SpecificRenoDXModAvailableWarning));
    }

    private void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    /* ---GAME CARD-------------------------------------------------------------------------------------------------------------- */
    [RelayCommand]
    private void OpenInExplorer(string? folder)
    {
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
        Task<ModOperationResultDto>> work, int delayMs = 5000)
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
        game.ReShadeModActionStatus = result.Message;
        game.IsShowingReShadeActionMessage = true;

        _ = DismissAsync();

        async Task DismissAsync()
        {
            try
            {
                await Task.Delay(delayMs, cts.Token);
            }
            catch (OperationCanceledException)
            {
            }

            game.ReShadeModActionStatus = null;
            game.IsShowingReShadeActionMessage = false;
        }
    }

    public string? ReShadeVersionTextColor =>
        SelectedGame?.HasReShade == true
            ? (CanShowReShadeUpdate ? s_upToDateTextColor : s_updateAvailableTextColor)
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

        await ExecuteReShadeActionAsync(p => _modManagementFacade.InstallOrUpdateReShadeAsync(request, p));
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

        await ExecuteReShadeActionAsync(p => _modManagementFacade.InstallOrUpdateReShadeAsync(request, p));
    }

    private bool CanUpdateReShade => CanShowReShadeUpdate;

    [RelayCommand(CanExecute = nameof(CanUninstallReShade))]
    private Task UninstallReShadeAsync() =>
        ExecuteReShadeActionAsync(_ => _modManagementFacade.UninstallReShadeAsync(SelectedGame!.GetGame()));

    private bool CanUninstallReShade => SelectedGame?.HasReShade ?? false;

    public bool CanShowReShadeUpdate =>
        SelectedGame?.HasReShade == true &&
        SelectedGame.ReShadeBranchName == SelectedReShadeBranch &&
        SelectedGame.ReShadeUpdateCheck?.UpdateAvailable == true;

    /* ---RENODX-------------------------------------------------------------------------------------------------------------- */
    private async Task InstallRenoDXAsync()
    {
        string? targetVersion = null;

        if (SelectedRenoDXBranch == RenoDX.Branch.Nightly)
        {
            var selectedTag = await _modSelectionDialogService.ShowRenoDXInstallDialogAsync();
            if (selectedTag is null) return;

            targetVersion = selectedTag.Version;
        }
        else if (SelectedRenoDXBranch == RenoDX.Branch.Snapshot)
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

        await ExecuteRenoDXActionAsync(p => _modManagementFacade.InstallOrUpdateRenoDXAsync(request, p));
    }

    private async Task ExecuteRenoDXActionAsync(
        Func<Progress<DownloadProgressReportDto>, Task<ModOperationResultDto>> work,
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
        game.RenoDXModActionStatus = result.Message;
        game.IsShowingRenoDXActionMessage = true;

        _ = DismissAsync();

        async Task DismissAsync()
        {
            try
            {
                await Task.Delay(delayMs, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            game.RenoDXModActionStatus = null;
            game.IsShowingRenoDXActionMessage = false;
        }
    }

    public string SpecificRenoDXModAvailableWarning =>
        SelectedGame?.IsUsingGenericModWhenSpecificAvailable == true
            ? """
              ⚡ A game-specific mod is now available!

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

            var modStatusText = RenoDXModStatus;
            var maintainerText = mod?.Maintainer is not null ? $"Maintainer: {mod.Maintainer}" : string.Empty;
            var extraNotes = genericMod?.Notes;

            if (!string.IsNullOrWhiteSpace(mod?.Maintainer))
                modStatusText += $"""


                                  {maintainerText}
                                  """;

            if (!string.IsNullOrWhiteSpace(extraNotes))
                modStatusText += $"""


                                  Additional notes:

                                  {extraNotes}
                                  """;

            return string.IsNullOrWhiteSpace(modStatusText) ? null : modStatusText;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RenoDXLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(RenoDXVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXUpdate))]
    [NotifyCanExecuteChangedFor(nameof(UpdateRenoDXCommand))]
    private RenoDX.Branch _selectedRenoDXBranch = RenoDX.Branch.Snapshot;

    private RenoDX.Branch _preferredRenoDXBranch = RenoDX.Branch.Snapshot;
    private bool _isAdjustingRenoDXBranchSelection;

    partial void OnSelectedRenoDXBranchChanged(RenoDX.Branch value)
    {
        if (!_isAdjustingRenoDXBranchSelection)
            _preferredRenoDXBranch = value;
    }

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanShowRenoDXBranchSelector))]
    private IReadOnlyList<RenoDX.Branch> _availableRenoDXBranches = [];

    public string? RenoDXLatestVersionForBranch =>
        _versionCatalog.GetLatestRenoDXVersionByTag(SelectedRenoDXBranch)?.Version;

    public bool CanShowRenoDXBranchSelector => AvailableRenoDXBranches.Count > 1;

    public string RenoDXBranchHelpText =>
        """
        Select the branch to use for RenoDX downloads.

        Snapshot: Default. Prefer using this branch.

        Nightly: Select this branch to rollback if latest Snapshot is causing issues.

        Wiki: Select this branch if Snapshot or Nightly fails to download or if it's otherwise preferable.
        """;

    private static IReadOnlyList<RenoDX.Branch> GetAvailableRenoDXBranches(GameModViewModel? game)
    {
        if (game is null)
            return [RenoDX.Branch.Snapshot];

        var hasWikiDownloadLink = game.RenoDXWikiDownloadUrl64 is not null || game.RenoDXWikiDownloadUrl32 is not null;
        var hasCompatibleMod = game.CompatibleRenoDXMod is not null;
        var isUnreal = !hasWikiDownloadLink && game.EngineName == Game.Engine.Unreal;
        var isUnity = !hasWikiDownloadLink && game.EngineName == Game.Engine.Unity;

        var branches = new List<RenoDX.Branch>();

        if (hasCompatibleMod || isUnreal)
        {
            branches.Add(RenoDX.Branch.Snapshot);
            branches.Add(RenoDX.Branch.Nightly);
        }

        if (hasWikiDownloadLink || isUnity)
            branches.Add(RenoDX.Branch.Wiki);

        return branches.Count > 0 ? branches : [RenoDX.Branch.Snapshot];
    }

    private void RefreshAvailableRenoDXBranches(GameModViewModel? game)
    {
        AvailableRenoDXBranches = GetAvailableRenoDXBranches(game);

        var target = AvailableRenoDXBranches.Contains(_preferredRenoDXBranch)
            ? _preferredRenoDXBranch
            : AvailableRenoDXBranches[0];

        if (target == SelectedRenoDXBranch)
            return;

        _isAdjustingRenoDXBranchSelection = true;
        SelectedRenoDXBranch = target;
        _isAdjustingRenoDXBranchSelection = false;
    }

    private string RenoDXModStatus =>
        (SelectedGame?.CompatibleRenoDXMod?.Status ?? SelectedGame?.CompatibleRenoDXGenericMod?.Status) switch
        {
            ":white_check_mark:" => "✅ Working",
            ":construction:" => "🚧 WIP, may lack testing or have deal-breaking issues",
            _ => string.Empty
        };

    public string? RenoDXVersionTextColor =>
        SelectedGame?.HasRenoDX == true
            ? (CanShowRenoDXUpdate ? s_upToDateTextColor : s_updateAvailableTextColor)
            : null;

    public string InstallRenoDXButtonText
    {
        get
        {
            if (SelectedGame?.HasRenoDX == true) return "Reinstall";
            if (CanOpenNexusLink) return "Get from Nexus";
            if (CanOpenDiscordLink) return "Get from Discord";

            return "Install";
        }
    }

    public string UpdateRenoDXButtonText => "Update";

    public string UninstallRenoDXButtonText => "Uninstall";

    [RelayCommand(CanExecute = nameof(CanClickRenoDXInstallButton))]
    private async Task RenoDXInstallButtonClickAsync()
    {
        if (CanOpenNexusLink)
        {
            OpenUrl(SelectedGame!.CompatibleRenoDXMod!.NexusUrl!);
            return;
        }

        if (CanOpenDiscordLink)
        {
            OpenUrl(SelectedGame!.CompatibleRenoDXMod!.DiscordUrl!);
            return;
        }

        await InstallRenoDXAsync();
    }

    private bool CanClickRenoDXInstallButton => CanInstallRenoDX || CanOpenNexusLink || CanOpenDiscordLink;

    private bool CanInstallRenoDX => SelectedGame is not null &&
                                     (SelectedGame.CompatibleRenoDXMod is not null ||
                                      SelectedGame.CompatibleRenoDXGenericMod is not null ||
                                      SelectedGame.EngineName == Game.Engine.Unity ||
                                      SelectedGame.EngineName == Game.Engine.Unreal ||
                                      SelectedGame.HasRenoDX) &&
                                     SelectedGame.HasReShade;

    private bool CanOpenNexusLink =>
        SelectedGame is
            { HasRenoDX: false, HasReShade: true, CompatibleRenoDXMod.HasWikiFilename: false, HasNexusLink: true };

    private bool CanOpenDiscordLink =>
        SelectedGame is
            { HasRenoDX: false, HasReShade: true, CompatibleRenoDXMod.HasWikiFilename: false, HasDiscordLink: true };

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

        await ExecuteRenoDXActionAsync(p => _modManagementFacade.InstallOrUpdateRenoDXAsync(request, p));
    }

    private bool CanUpdateRenoDX => CanShowRenoDXUpdate;

    [RelayCommand(CanExecute = nameof(CanUninstallRenoDX))]
    private Task UninstallRenoDXAsync() =>
        ExecuteRenoDXActionAsync(_ => _modManagementFacade.UninstallRenoDXAsync(SelectedGame!.GetGame()));

    private bool CanUninstallRenoDX => SelectedGame?.HasRenoDX ?? false;

    public bool CanShowRenoDXUpdate =>
        SelectedGame?.HasRenoDX == true &&
        SelectedGame.EngineName != Game.Engine.Unity &&
        SelectedRenoDXBranch != RenoDX.Branch.Wiki &&
        SelectedGame.RenoDXBranchName == SelectedRenoDXBranch &&
        SelectedGame.RenoDXUpdateCheck?.UpdateAvailable == true;
}