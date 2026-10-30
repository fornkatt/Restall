// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Restall.UI.ViewModels;

public sealed partial class GameModViewModel : ObservableObject, IDisposable
{
    private const int ThumbnailTargetWidth = 32;

    private ImmutableArray<string> _seenRenoDXNotices = [];

    public GameModViewModel(GameEntry gameEntry)
    {
        GameEntry = gameEntry;

        Thumbnail = new AsyncBitmap(Game.ThumbnailPathString, ThumbnailTargetWidth);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RenoDXEntry))]
    [NotifyPropertyChangedFor(nameof(InstallRenoDXButtonText))]
    [NotifyPropertyChangedFor(nameof(CanUseRenoDXInstallButton))]
    [NotifyPropertyChangedFor(nameof(HasUnseenRenoDXNotices))]
    [NotifyPropertyChangedFor(nameof(IsRenoDXInfoBoxVisible))]
    [NotifyPropertyChangedFor(nameof(ReShadeUpdateCheck))]
    public partial GameEntry GameEntry { get; set; }

    public UpdateAvailability? ReShadeUpdateCheck => GameEntry.ReShadeUpdate;

    public string? Name => Game.Name;
    private Game Game => GameEntry.Game;
    internal Game GetGame() => Game;
    public bool HasReShade => Game.HasReShade;
    public Game.Platform PlatformName => Game.PlatformName;
    public Game.Engine EngineName => Game.EngineName;
    public string? ExecutablePath => Game.ExecutablePath;

    public string? ExecutablePathDisplay => OperatingSystem.IsWindows()
        ? ExecutablePath?.Replace(@"\", "\\\u200B")
        : ExecutablePath?.Replace("/", "/\u200B");

    public string? InstallFolder => Game.InstallFolder;

    public string? InstallFolderDisplay => OperatingSystem.IsWindows()
        ? InstallFolder?.Replace(@"\", "\\\u200B")
        : InstallFolder?.Replace("/", "/\u200B");

    internal void NotifyGameStateChanged()
    {
        OnPropertyChanged(nameof(ReShadeBranchName));
        OnPropertyChanged(nameof(ReShadeVersion));
        OnPropertyChanged(nameof(ReShadeBranch));
        OnPropertyChanged(nameof(ReShadeArch));
        OnPropertyChanged(nameof(ReShadeFilename));

        OnPropertyChanged(nameof(RenoDXName));
        OnPropertyChanged(nameof(RenoDXVersion));
        OnPropertyChanged(nameof(RenoDXBranch));
        OnPropertyChanged(nameof(RenoDXBranchName));
        OnPropertyChanged(nameof(HasRenoDX));
        OnPropertyChanged(nameof(HasReShade));
        OnPropertyChanged(nameof(CanUseRenoDXInstallButton));
    }

    // ReShade -------------------------------------------------------------------------------

    public string? ReShadeVersion => Game.ReShade?.Version;
    public string? ReShadeBranch => Game.ReShade?.BranchName.ToString();
    public ReShade.Branch? ReShadeBranchName => Game.ReShade?.BranchName;
    public string? ReShadeArch => Game.ReShade?.Arch.ToString();
    public string? ReShadeFilename => Game.ReShade?.SelectedFilename;

    // RenoDX --------------------------------------------------------------------------------

    public RenoDXEntry RenoDXEntry => GameEntry.RenoDXEntry;

    public bool CanUseRenoDXInstallButton =>
        HasReShade && RenoDXEntry is { DownloadOptions: not null } or { ManualSource: not null };

    public RenoDX.Branch? LastSelectedRenoDXBranch { get; set; }

    public bool HasUnseenRenoDXNotices => RenoDXEntry.Notices.Except(_seenRenoDXNotices).Any();

    public bool HasRenoDX => Game.HasRenoDX;
    public string? RenoDXName => Game.RenoDX?.SelectedName;
    public string? RenoDXVersion => Game.RenoDX?.Version;
    public string? RenoDXBranch => Game.RenoDX?.BranchName.ToString();
    public RenoDX.Branch? RenoDXBranchName => Game.RenoDX?.BranchName;

    public string InstallRenoDXButtonText => RenoDXEntry switch
    {
        { ManualSource: { } manualSource } => $"Get from {manualSource.Label}",
        { DownloadOptions.IsInstalledFile: true, GenericAddonInfo: not null, } => "Reinstall generic mod",
        { DownloadOptions.IsInstalledFile: true } => "Reinstall",
        { DownloadOptions: not null } when HasRenoDX => $"Replace with {GetBitnessText()} build",
        { GenericAddonInfo: not null } => "Install generic mod",
        _ => "Install"
    };

    public string UpdateRenoDXButtonText => "Update";

    public string UninstallRenoDXButtonText => "Uninstall";

    private string GetBitnessText() => GameEntry.RecommendedArchitecture switch
    {
        Architecture.X64 => "64-bit",
        Architecture.X32 => "32-bit",
        _ => throw new UnreachableException($"No text for bitness \"{GameEntry.RecommendedArchitecture}\"")
    };

    [RelayCommand]
    private void MarkRenoDXNoticesSeen()
    {
        _seenRenoDXNotices = RenoDXEntry.Notices;
        OnPropertyChanged(nameof(HasUnseenRenoDXNotices));
    }

    // Action messages -----------------------------------------------------------------------

    [ObservableProperty] public partial string? ReShadeModActionStatus { get; set; }
    [ObservableProperty] public partial bool IsShowingReShadeActionMessage { get; set; }
    internal CancellationTokenSource? ReShadeMessageCts { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsShowingRenoDXMessage))]
    [NotifyPropertyChangedFor(nameof(IsRenoDXInfoBoxVisible))]
    public partial string? RenoDXMessage { get; set; }

    public bool IsRenoDXInfoBoxVisible => RenoDXEntry.IsSupported || IsShowingRenoDXMessage;

    public bool IsShowingRenoDXMessage => RenoDXMessage is not null;

    internal CancellationTokenSource? RenoDXMessageCts { get; set; }

    // Bitmaps -------------------------------------------------------------------------------

    public string? CoverPath => Game.GameCoverPathString;
    public AsyncBitmap Thumbnail { get; }

    public void Dispose() => Thumbnail.Dispose();
}
