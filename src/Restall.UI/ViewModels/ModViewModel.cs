// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restall.Application.DTOs;
using Restall.Application.DTOs.Requests;
using Restall.Application.DTOs.Responses;
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

namespace Restall.UI.ViewModels;

public sealed partial class ModViewModel : ViewModelBase
{
    private readonly IModManagementFacade _modManagementFacade;
    private readonly IModSelectionDialogService _modSelectionDialogService;
    private readonly IVersionCatalog _versionCatalog;

    private const string UpToDateTextColor = "#eb5a2f";
    private const string UpdateAvailableTextColor = "#1ab652";

    private const int ActionMessageDurationMs = 5000;

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
    [NotifyPropertyChangedFor(nameof(ReShadeVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowReShadeUpdate))]
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXBranchSelector))]
    [NotifyPropertyChangedFor(nameof(AvailableRenoDXBranches))]
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXUpdate))]
    [NotifyPropertyChangedFor(nameof(RenoDXVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(RenoDXLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(IsRenoDXUpdateCheckUnavailable))]
    public partial GameModViewModel? SelectedGame { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ReShadeLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(ReShadeVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowReShadeUpdate))]
    [NotifyCanExecuteChangedFor(nameof(UpdateReShadeCommand))]
    public partial ReShade.Branch SelectedReShadeBranch { get; set; } = ReShade.Branch.Stable;

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
        OnPropertyChanged(nameof(ReShadeVersionTextColor));
        OnPropertyChanged(nameof(InstallReShadeButtonText));
        OnPropertyChanged(nameof(UpdateReShadeButtonText));
        OnPropertyChanged(nameof(UninstallReShadeButtonText));

        OnPropertyChanged(nameof(CanShowRenoDXUpdate));
        OnPropertyChanged(nameof(RenoDXVersionTextColor));
        OnPropertyChanged(nameof(RenoDXLatestVersionForBranch));
        OnPropertyChanged(nameof(IsRenoDXUpdateCheckUnavailable));
        OnPropertyChanged(nameof(RenoDXUpdateArrowText));
    }

    private void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
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
                FileName = "explorer.exe", Arguments = $"\"{folder}\"", UseShellExecute = false
            });
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open", ArgumentList = { folder }, UseShellExecute = false
            });
        }
    }

    /* ---RESHADE-------------------------------------------------------------------------------------------------------------- */
    private async Task ExecuteReShadeActionAsync(Func<Progress<DownloadProgressReport>,
        Task<ModOperationResponse>> work)
    {
        var game = SelectedGame!;

        game._reShadeMessageCts?.Cancel();
        var cts = new CancellationTokenSource();
        game._reShadeMessageCts = cts;

        var progress = new Progress<DownloadProgressReport>(report =>
        {
            game.ReShadeModActionStatus = report.PercentComplete >= 0
                ? $"""
                   Downloading {report.Filename}
                   {report.PercentComplete}%
                   """
                : $"Downloading {report.Filename}";
            game.IsShowingReShadeActionMessage = true;
        });

        var result = await Task.Run(() => work(progress));

        if (result.GameEntry is not null)
            game.GameEntry = result.GameEntry;

        game.ReShadeUpdateCheck = result.UpdateCheckResult;
        game.NotifyGameStateChanged();
        RefreshAvailableRenoDXBranches(SelectedGame);
        NotifyAllCommandsChanged();
        game.ReShadeModActionStatus = result.Message;
        game.IsShowingReShadeActionMessage = true;

        _ = DismissAsync();

        async Task DismissAsync()
        {
            try
            {
                await Task.Delay(ActionMessageDurationMs, cts.Token);
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
            ? (CanShowReShadeUpdate ? UpToDateTextColor : UpdateAvailableTextColor)
            : null;

    public string InstallReShadeButtonText =>
        SelectedGame?.HasReShade == true ? "Reinstall" : "Install";

    public string UpdateReShadeButtonText => "Update";

    public string UninstallReShadeButtonText => "Uninstall";

    [RelayCommand(CanExecute = nameof(CanInstallReShade))]
    private async Task InstallReShadeAsync()
    {
        if (SelectedGame?.GameEntry is not { } gameEntry)
            return;

        var selection = await _modSelectionDialogService.ShowReShadeInstallDialogAsync();

        if (selection is null)
            return;

        var request = new InstallReShadeRequest(
            SelectedGame!.GetGame(),
            SelectedReShadeBranch,
            gameEntry.RecommendedArchitecture,
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

        if (installedFilename is null || latestVersion is null || SelectedGame?.GameEntry is not { } gameEntry)
            return;

        var request = new InstallReShadeRequest(
            SelectedGame!.GetGame(),
            SelectedReShadeBranch,
            gameEntry.RecommendedArchitecture,
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
        var game = SelectedGame!;

        if (SelectedRenoDXBranch is not { } branch)
            return;

        string? nightlyVersion = null;

        if (branch is RenoDX.Branch.Nightly)
        {
            var nightlies = game.RenoDXEntry?.DownloadOptions?.Nightlies ?? [];

            var selectedTag = await _modSelectionDialogService.ShowRenoDXInstallDialogAsync(nightlies);

            if (selectedTag is null)
                return;

            nightlyVersion = selectedTag.Version;
        }

        await RunRenoDXInstallAsync(game,
            new RenoDXInstallRequest(game.GetGame(), branch, nightlyVersion));
    }

    private async Task RunRenoDXInstallAsync(GameModViewModel game, RenoDXInstallRequest request)
    {
        var messageToken = BeginRenoDXAction(game);
        var progress = CreateRenoDXDownloadProgress(game);
        var response = await Task.Run(() => _modManagementFacade.InstallOrUpdateRenoDXAsync(request, progress));

        ShowRenoDXResponse(game, response.Message, response.GameEntry, messageToken);
    }

    private static CancellationToken BeginRenoDXAction(GameModViewModel game)
    {
        game._renoDXMessageCts?.Cancel();
        game._renoDXMessageCts = new CancellationTokenSource();

        return game._renoDXMessageCts.Token;
    }

    private static Progress<DownloadProgressReport> CreateRenoDXDownloadProgress(GameModViewModel game) =>
        new(report =>
        {
            game.RenoDXModActionStatus = report.PercentComplete >= 0
                ? $"""
                   Downloading {report.Filename}
                   {report.PercentComplete}%
                   """
                : $"Downloading {report.Filename}";
        });

    private void ShowRenoDXResponse(GameModViewModel game, string? message, GameEntry? gameEntry,
        CancellationToken messageToken)
    {
        if (gameEntry is not null)
            game.GameEntry = gameEntry;

        game.NotifyGameStateChanged();
        RefreshAvailableRenoDXBranches(SelectedGame);
        NotifyAllCommandsChanged();
        game.RenoDXModActionStatus = message;
        _ = DismissRenoDXActionMessageAsync(game, messageToken);
    }

    private static async Task DismissRenoDXActionMessageAsync(GameModViewModel game, CancellationToken messageToken)
    {
        try
        {
            await Task.Delay(ActionMessageDurationMs, messageToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        game.RenoDXModActionStatus = null;
    }

    private bool _isAdjustingRenoDXBranchSelection;

    private UpdateAvailability? SelectedRenoDXBranchVersion =>
        SelectedRenoDXBranch is { } branch
            ? SelectedGame?.RenoDXEntry?.BranchVersions.GetValueOrDefault(branch)
            : null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RenoDXLatestVersionForBranch))]
    [NotifyPropertyChangedFor(nameof(RenoDXVersionTextColor))]
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXUpdate))]
    [NotifyPropertyChangedFor(nameof(IsRenoDXUpdateCheckUnavailable))]
    [NotifyPropertyChangedFor(nameof(RenoDXUpdateArrowText))]
    [NotifyCanExecuteChangedFor(nameof(UpdateRenoDXCommand))]
    public partial RenoDX.Branch? SelectedRenoDXBranch { get; set; }

    partial void OnSelectedRenoDXBranchChanged(RenoDX.Branch? value)
    {
        if (!_isAdjustingRenoDXBranchSelection && value is { } branch && SelectedGame is { } game)
            game.LastSelectedRenoDXBranch = value;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanShowRenoDXBranchSelector))]
    public partial IReadOnlyList<RenoDX.Branch> AvailableRenoDXBranches { get; set; } = [];

    public string? RenoDXLatestVersionForBranch => SelectedRenoDXBranchVersion?.LatestVersion;

    public string? RenoDXUpdateArrowText => CanShowRenoDXUpdate ? $"-> {RenoDXLatestVersionForBranch}" : null;

    public bool IsRenoDXUpdateCheckUnavailable =>
        SelectedGame?.HasRenoDX == true && SelectedRenoDXBranchVersion is { IsSupported: false };

    public string RenoDXUpdateCheckUnavailableText => "Update checks are not available for this branch.";

    public bool CanShowRenoDXBranchSelector => AvailableRenoDXBranches.Count > 1;

    public static string RenoDXBranchHelpText =>
        """
        Select the branch to use for RenoDX downloads.

        Snapshot: Default. Prefer using this branch.

        Nightly: Select this branch to rollback if latest Snapshot is causing issues.

        Wiki: Select this branch if Snapshot or Nightly fails to download or if it's otherwise preferable.
        """;

    private void RefreshAvailableRenoDXBranches(GameModViewModel? game)
    {
        _isAdjustingRenoDXBranchSelection = true;

        AvailableRenoDXBranches = game?.RenoDXEntry?.DownloadOptions?.Branches ?? [];
        SelectedRenoDXBranch = GetRenoDXBranchToSelect(game, AvailableRenoDXBranches);

        OnPropertyChanged(nameof(SelectedRenoDXBranch));

        _isAdjustingRenoDXBranchSelection = false;
    }

    private static RenoDX.Branch? GetRenoDXBranchToSelect(GameModViewModel? game,
        IReadOnlyList<RenoDX.Branch> branches) => game switch
    {
        { LastSelectedRenoDXBranch: { } lastSelected } when branches.Contains(lastSelected) => lastSelected,
        { RenoDXBranchName: { } installedBranch } when branches.Contains(installedBranch) => installedBranch,
        _ when branches.Count > 0 => branches[0],
        _ => null
    };

    public string? RenoDXVersionTextColor =>
        SelectedGame?.HasRenoDX == true
            ? (CanShowRenoDXUpdate ? UpToDateTextColor : UpdateAvailableTextColor)
            : null;

    [RelayCommand(CanExecute = nameof(CanClickRenoDXInstallButton))]
    private async Task RenoDXInstallButtonClickAsync()
    {
        if (SelectedGame?.RenoDXEntry is { DownloadOptions: null, ManualSource: { } manualSource })
        {
            OpenUrl(manualSource.Url.AbsoluteUri);

            return;
        }

        await InstallRenoDXAsync();
    }

    private bool CanClickRenoDXInstallButton => SelectedGame?.CanUseRenoDXInstallButton == true;

    private bool CanUpdateRenoDX => CanShowRenoDXUpdate;

    [RelayCommand(CanExecute = nameof(CanUpdateRenoDX))]
    private async Task UpdateRenoDXAsync()
    {
        var game = SelectedGame!;

        if (SelectedRenoDXBranch is not { } branch)
            return;

        await RunRenoDXInstallAsync(game, new RenoDXInstallRequest(game.GetGame(), branch));
    }

    private bool CanUninstallRenoDX => SelectedGame?.HasRenoDX ?? false;

    [RelayCommand(CanExecute = nameof(CanUninstallRenoDX))]
    private async Task UninstallRenoDXAsync()
    {
        var game = SelectedGame!;
        var gameModel = game.GetGame();
        var messageToken = BeginRenoDXAction(game);
        var response = await Task.Run(() => _modManagementFacade.UninstallRenoDXAsync(gameModel));

        ShowRenoDXResponse(game, response.Message, response.GameEntry, messageToken);
    }

    public bool CanShowRenoDXUpdate =>
        SelectedGame?.HasRenoDX == true && SelectedRenoDXBranchVersion?.UpdateAvailable == true;
}
