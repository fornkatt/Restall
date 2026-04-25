using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Application.DTOs;
using Restall.Application.Helpers;
using Restall.Domain.Entities;
using System;
using System.IO;
using System.Threading;
using Restall.Application.DTOs.Results;

namespace Restall.UI.ViewModels;

/// <summary>
/// We wrap a Game domain entity in a ViewModel and flatten the Game entity's properties into simpler types
/// that we can easily bind to our UI and use in our other ViewModels to calculate for example button visibilty
/// and text/notes visibility
/// </summary>
public sealed partial class GameModViewModel : ObservableObject
{
    private readonly Game _game;

    private const int s_bannerTargetWidth = 1000;
    private const int s_logoTargetWidth = 300;
    private const int s_thumbnailTargetWidth = 32;

    private Lazy<Bitmap?> _bannerBitmap;
    private Lazy<Bitmap?> _logoBitmap;
    private Lazy<Bitmap?> _thumbnailBitmap;

    public GameModViewModel(Game game)
    {
        _game = game;
        _bannerPathString = game.BannerPathString;
        _logoPathString = game.LogoPathString;
        _thumbnailPathString = game.ThumbnailPathString;
        NormalizedName = GameNameHelper.NormalizeName(game.Name!);

        _bannerBitmap = CreateLazyBitmap(_bannerPathString, s_bannerTargetWidth);
        _logoBitmap = CreateLazyBitmap(_logoPathString, s_logoTargetWidth);
        _thumbnailBitmap = CreateLazyBitmap(_thumbnailPathString, s_thumbnailTargetWidth);
    }

    [ObservableProperty]
    private UpdateCheckResultDto? _reShadeUpdateCheck;
    
    [ObservableProperty]
    private UpdateCheckResultDto? _renoDXUpdateCheck;
    
    public string NormalizedName { get; }
    public string? Name => _game.Name;
    public Game.Platform PlatformName => _game.PlatformName;
    public Game.Engine EngineName => _game.EngineName;
    public string? ExecutablePath => _game.ExecutablePath;
    public string? InstallFolder => _game.InstallFolder;
    public bool HasRenoDX => _game.HasRenoDX;
    public bool HasReShade => _game.HasReShade;
    public bool IsRenoDXSupported =>
        (CompatibleRenoDXMod is not null            ||
         CompatibleRenoDXGenericMod is not null)    ||
        (EngineName == Game.Engine.Unity            ||
         EngineName == Game.Engine.Unreal)          ||
         HasRenoDX;
    public string? ReShadeVersion => _game.ReShade?.Version;
    public string? ReShadeBranch => _game.ReShade?.BranchName.ToString();
    public ReShade.Branch? ReShadeBranchName => _game.ReShade?.BranchName;
    public string? ReShadeArch => _game.ReShade?.Arch.ToString();
    public string? ReShadeFilename => _game.ReShade?.SelectedFilename;
    public bool IsUsingGenericModWhenSpecificAvailable =>
        HasRenoDX                                               &&
        CompatibleRenoDXMod is { HasWikiFilename: true } mod    &&
        _game.RenoDX?.OriginalName is { } installedName         &&
        installedName != mod.AddonFilename64                    &&
        installedName != mod.AddonFilename32;

    // Get the actual game object
    internal Game GetGame() => _game;

    // Call when installing or uninstalling ReShade/RenoDX to notify the VM these have changed since they are derived from _game
    internal void NotifyGameStateChanged()
    {
        OnPropertyChanged(nameof(RenoDXBranchName));
        OnPropertyChanged(nameof(ReShadeBranchName));
        OnPropertyChanged(nameof(HasRenoDX));
        OnPropertyChanged(nameof(HasReShade));
        OnPropertyChanged(nameof(IsRenoDXSupported));
        OnPropertyChanged(nameof(IsUsingGenericModWhenSpecificAvailable));
        OnPropertyChanged(nameof(ReShadeVersion));
        OnPropertyChanged(nameof(ReShadeBranch));
        OnPropertyChanged(nameof(ReShadeArch));
        OnPropertyChanged(nameof(ReShadeFilename));
        OnPropertyChanged(nameof(RenoDXName));
        OnPropertyChanged(nameof(RenoDXVersion));
        OnPropertyChanged(nameof(RenoDXBranch));
        OnPropertyChanged(nameof(RenoDXArch));
    }

    public string? RenoDXName => _game.RenoDX?.SelectedName;
    public string? RenoDXVersion => _game.RenoDX?.Version;
    public string? RenoDXBranch => _game.RenoDX?.BranchName.ToString();
    public RenoDX.Branch? RenoDXBranchName => _game.RenoDX?.BranchName;
    public string? RenoDXArch => _game.RenoDX?.Arch.ToString();

    public bool RenoDXSupportsX64 => CompatibleRenoDXMod?.SupportsX64 ?? false;
    public bool RenoDXSupportsX32 => CompatibleRenoDXMod?.SupportsX32 ?? false;
    public bool RenoDXIsDualArch => CompatibleRenoDXMod?.IsDualArch ?? false;

    public string? RenoDXAddonFilenameX64 => CompatibleRenoDXMod?.AddonFilename64;
    public string? RenoDXAddonFilenameX32 => CompatibleRenoDXMod?.AddonFilename32;

    [ObservableProperty]
    private string? _reShadeModActionStatus;
    
    [ObservableProperty]
    private bool _isShowingReShadeActionMessage;
    
    internal CancellationTokenSource? _reShadeMessageCts;
    
    [ObservableProperty]
    private string? _renoDXModActionStatus;
    
    [ObservableProperty]
    private bool _isShowingRenoDXActionMessage;

    internal CancellationTokenSource? _renoDXMessageCts;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRenoDXInstallArch))]
    [NotifyPropertyChangedFor(nameof(SelectedReShadeInstallArch))]
    [NotifyPropertyChangedFor(nameof(RenoDXWikiDownloadUrl))]
    [NotifyPropertyChangedFor(nameof(RenoDXAddonFilename))]
    private RenoDX.Architecture? _archOverride;

    public RenoDX.Architecture SelectedRenoDXInstallArch =>
        ArchOverride ?? (CompatibleRenoDXMod?.SupportsX32 == true && CompatibleRenoDXMod?.SupportsX64 != true
            ? RenoDX.Architecture.x32
            : RenoDX.Architecture.x64);

    public ReShade.Architecture SelectedReShadeInstallArch =>
        SelectedRenoDXInstallArch == RenoDX.Architecture.x32
            ? ReShade.Architecture.x32
            : ReShade.Architecture.x64;

    public string? RenoDXWikiDownloadUrl =>
        SelectedRenoDXInstallArch == RenoDX.Architecture.x32
            ? CompatibleRenoDXMod?.SnapshotUrl32 ?? CompatibleRenoDXMod?.SnapshotUrl64
            : CompatibleRenoDXMod?.SnapshotUrl64 ?? CompatibleRenoDXMod?.SnapshotUrl32;

    public string? RenoDXAddonFilename =>
        SelectedRenoDXInstallArch == RenoDX.Architecture.x32
            ? CompatibleRenoDXMod?.AddonFilename32 ?? CompatibleRenoDXMod?.AddonFilename64
            : CompatibleRenoDXMod?.AddonFilename64 ?? CompatibleRenoDXMod?.AddonFilename32;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRenoDXSupported))]
    [NotifyPropertyChangedFor(nameof(IsUsingGenericModWhenSpecificAvailable))]
    [NotifyPropertyChangedFor(nameof(SelectedRenoDXInstallArch))]
    [NotifyPropertyChangedFor(nameof(SelectedReShadeInstallArch))]
    [NotifyPropertyChangedFor(nameof(RenoDXWikiDownloadUrl))]
    [NotifyPropertyChangedFor(nameof(RenoDXAddonFilename))]
    [NotifyPropertyChangedFor(nameof(RenoDXSupportsX64))]
    [NotifyPropertyChangedFor(nameof(RenoDXSupportsX32))]
    [NotifyPropertyChangedFor(nameof(RenoDXIsDualArch))]
    [NotifyPropertyChangedFor(nameof(RenoDXAddonFilenameX64))]
    [NotifyPropertyChangedFor(nameof(RenoDXAddonFilenameX32))]
    private RenoDXModInfoDto? _compatibleRenoDXMod;

    partial void OnCompatibleRenoDXModChanged(RenoDXModInfoDto? value) => ArchOverride = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRenoDXSupported))]
    private RenoDXGenericModInfoDto? _compatibleRenoDXGenericMod;

    // Bitmaps -------------------------------------------------------------------------------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BannerBitmap))]
    private string? _bannerPathString;

    partial void OnBannerPathStringChanged(string? value) =>
        ResetLazyBitmap(ref _bannerBitmap, value, s_bannerTargetWidth);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LogoBitmap))]
    private string? _logoPathString;

    partial void OnLogoPathStringChanged(string? value) =>
        ResetLazyBitmap(ref _logoBitmap, value, s_logoTargetWidth);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThumbnailBitmap))]
    private string? _thumbnailPathString;

    partial void OnThumbnailPathStringChanged(string? value) =>
        ResetLazyBitmap(ref _thumbnailBitmap, value, s_thumbnailTargetWidth);

    public Bitmap? BannerBitmap => _bannerBitmap.Value;
    public Bitmap? LogoBitmap => _logoBitmap.Value;
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