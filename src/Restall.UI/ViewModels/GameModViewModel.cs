// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Restall.UI.ViewModels;

public sealed partial class GameModViewModel : ObservableObject
{
    private readonly Game _game;

    private const int CoverTargetWidth = 600;
    private const int ThumbnailTargetWidth = 32;

    private Lazy<Bitmap?> _coverBitMap = CreateLazyBitmap(null, CoverTargetWidth);
    private Lazy<Bitmap?> _thumbnailBitmap = CreateLazyBitmap(null, ThumbnailTargetWidth);

    private ImmutableArray<string> _seenRenoDXNotices = [];

    public GameModViewModel(Game game)
    {
        _game = game;

        CoverPathString = game.GameCoverPathString;
        ThumbnailPathString = game.ThumbnailPathString;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RenoDXEntry))]
    [NotifyPropertyChangedFor(nameof(IsRenoDXSupported))]
    [NotifyPropertyChangedFor(nameof(InstallRenoDXButtonText))]
    [NotifyPropertyChangedFor(nameof(CanUseRenoDXInstallButton))]
    [NotifyPropertyChangedFor(nameof(HasRenoDXNotices))]
    [NotifyPropertyChangedFor(nameof(RenoDXNoticeCount))]
    [NotifyPropertyChangedFor(nameof(HasUnseenRenoDXNotices))]
    [NotifyPropertyChangedFor(nameof(HasRenoDXGenericAddonInfo))]
    [NotifyPropertyChangedFor(nameof(IsRenoDXStatusDone))]
    [NotifyPropertyChangedFor(nameof(IsRenoDXStatusWorkInProgress))]
    public partial GameEntry? GameEntry { get; set; }

    [ObservableProperty] public partial UpdateAvailability? ReShadeUpdateCheck { get; set; }

    public string? Name => _game.Name;
    internal Game GetGame() => _game;
    public bool HasReShade => _game.HasReShade;
    public Game.Platform PlatformName => _game.PlatformName;
    public Game.Engine EngineName => _game.EngineName;
    public string? ExecutablePath => _game.ExecutablePath;

    public string? ExecutablePathDisplay => OperatingSystem.IsWindows()
        ? ExecutablePath?.Replace(@"\", "\\\u200B")
        : ExecutablePath?.Replace("/", "/\u200B");

    public string? InstallFolder => _game.InstallFolder;

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
        OnPropertyChanged(nameof(RenoDXArch));
        OnPropertyChanged(nameof(HasRenoDX));
        OnPropertyChanged(nameof(HasReShade));
        OnPropertyChanged(nameof(IsRenoDXSupported));
        OnPropertyChanged(nameof(CanUseRenoDXInstallButton));
    }

    // ReShade -------------------------------------------------------------------------------

    public string? ReShadeVersion => _game.ReShade?.Version;
    public string? ReShadeBranch => _game.ReShade?.BranchName.ToString();
    public ReShade.Branch? ReShadeBranchName => _game.ReShade?.BranchName;
    public string? ReShadeArch => _game.ReShade?.Arch.ToString();
    public string? ReShadeFilename => _game.ReShade?.SelectedFilename;

    // RenoDX --------------------------------------------------------------------------------

    public RenoDXEntry? RenoDXEntry => GameEntry?.RenoDXEntry;

    public bool IsRenoDXSupported => RenoDXEntry?.IsSupported ?? false;

    public bool CanUseRenoDXInstallButton =>
        HasReShade && RenoDXEntry is { DownloadOptions: not null } or { ManualSource: not null };

    public bool HasRenoDXGenericAddonInfo => RenoDXEntry?.GenericAddonInfo is not null;
    public bool IsRenoDXStatusDone => RenoDXEntry is { IsDone: true };
    public bool IsRenoDXStatusWorkInProgress => RenoDXEntry is { IsWorkInProgress: true };
    public RenoDX.Branch? LastSelectedRenoDXBranch { get; set; }
    public bool HasRenoDXNotices => RenoDXEntry is { Notices.IsEmpty: false };
    public int RenoDXNoticeCount => RenoDXEntry?.Notices.Length ?? 0;

    public bool HasUnseenRenoDXNotices =>
        RenoDXEntry is { } renoDXEntry && renoDXEntry.Notices.Except(_seenRenoDXNotices).Any();

    public bool HasRenoDX => _game.HasRenoDX;
    public string? RenoDXName => _game.RenoDX?.SelectedName;
    public string? RenoDXVersion => _game.RenoDX?.Version;
    public string? RenoDXBranch => _game.RenoDX?.BranchName.ToString();
    public RenoDX.Branch? RenoDXBranchName => _game.RenoDX?.BranchName;
    public string? RenoDXArch => _game.RenoDX?.Arch.ToString();

    public string InstallRenoDXButtonText => RenoDXEntry switch
    {
        { DownloadOptions: null, ManualSource: { } manualSource } => $"Get from {manualSource.Label}",
        { DownloadOptions.IsInstalledFile: true, GenericAddonInfo: not null, } => "Reinstall generic mod",
        { DownloadOptions.IsInstalledFile: true } => "Reinstall",
        { DownloadOptions: not null } when HasRenoDX => $"Replace with {GetBitnessText()} build",
        { GenericAddonInfo: not null } => "Install generic mod",
        _ => "Install"
    };

    public string UpdateRenoDXButtonText => "Update";

    public string UninstallRenoDXButtonText => "Uninstall";

    private string GetBitnessText() => GameEntry?.RecommendedArchitecture switch
    {
        Architecture.X64 => "64-bit",
        Architecture.X32 => "32-bit",
        _ => throw new UnreachableException($"No text for bitness \"{GameEntry?.RecommendedArchitecture}\"")
    };

    [RelayCommand]
    private void MarkRenoDXNoticesSeen()
    {
        _seenRenoDXNotices = RenoDXEntry?.Notices ?? [];
        OnPropertyChanged(nameof(HasUnseenRenoDXNotices));
    }

    // Action messages -----------------------------------------------------------------------

    [ObservableProperty] public partial string? ReShadeModActionStatus { get; set; }
    [ObservableProperty] public partial bool IsShowingReShadeActionMessage { get; set; }
    internal CancellationTokenSource? _reShadeMessageCts;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsShowingRenoDXActionMessage))]
    public partial string? RenoDXModActionStatus { get; set; }

    public bool IsShowingRenoDXActionMessage => RenoDXModActionStatus is not null;
    internal CancellationTokenSource? _renoDXMessageCts;

    // Bitmaps -------------------------------------------------------------------------------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoverBitmap))]
    public partial string? CoverPathString { get; set; }

    partial void OnCoverPathStringChanged(string? value) =>
        ResetLazyBitmap(ref _coverBitMap, value, CoverTargetWidth);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThumbnailBitmap))]
    public partial string? ThumbnailPathString { get; set; }

    partial void OnThumbnailPathStringChanged(string? value) =>
        ResetLazyBitmap(ref _thumbnailBitmap, value, ThumbnailTargetWidth);

    public Bitmap? CoverBitmap => _coverBitMap.Value;
    public Bitmap? ThumbnailBitmap => _thumbnailBitmap.Value;

    private static Lazy<Bitmap?> CreateLazyBitmap(string? path, int targetWidth) =>
        new(() => DecodeBitmap(path, targetWidth), LazyThreadSafetyMode.None);

    private static Bitmap? DecodeBitmap(string? path, int targetWidth)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        using var stream = File.OpenRead(path);

        return Bitmap.DecodeToWidth(stream, targetWidth);
    }

    private void ResetLazyBitmap(ref Lazy<Bitmap?> lazy, string? newPath, int targetWidth)
    {
        if (lazy.IsValueCreated)
            lazy.Value?.Dispose();

        lazy = CreateLazyBitmap(newPath, targetWidth);
    }
}
