using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Application.DTOs;
using Restall.Application.Helpers;
using Restall.Domain.Entities;
using System;
using System.IO;

namespace Restall.UI.ViewModels;

public partial class GameModViewModel : ObservableObject
{
    private readonly Game _game;

    private const int BannerTargetWidth = 1000;
    private const int LogoTargetWidth = 300;
    private const int ThumbnailTargetWidth = 32;

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

        _bannerBitmap = CreateLazyBitmap(_bannerPathString, BannerTargetWidth);
        _logoBitmap = CreateLazyBitmap(_logoPathString, LogoTargetWidth);
        _thumbnailBitmap = CreateLazyBitmap(_thumbnailPathString, ThumbnailTargetWidth);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanUpdateReShade))]
    [NotifyPropertyChangedFor(nameof(ReShadeLatestVersion))]
    private UpdateCheckResultDto? _reShadeUpdateResult;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanUpdateRenoDX))]
    [NotifyPropertyChangedFor(nameof(RenoDXLatestVersion))]
    private UpdateCheckResultDto? _renoDXUpdateResult;

    public string NormalizedName { get; }
    public string? Name => _game.Name;
    public Game.Platform PlatformName => _game.PlatformName;
    public Game.Engine EngineName => _game.EngineName;
    public string? ExecutablePath => _game.ExecutablePath;
    public string? InstallFolder => _game.InstallFolder;
    public bool HasRenoDX => _game.HasRenoDX;
    public bool HasReShade => _game.HasReShade;
    public bool CanInstallRenoDX => (CompatibleRenoDXMod is not null ||
                                     CompatibleRenoDXGenericMod is not null ||
                                     EngineName == Game.Engine.Unity ||
                                     EngineName == Game.Engine.Unreal) && 
                                     HasReShade;
    public bool CanUpdateReShade => HasReShade && (ReShadeUpdateResult?.UpdateAvailable ?? false);
    public string? ReShadeLatestVersion => ReShadeUpdateResult?.LatestVersion;
    public bool CanUpdateRenoDX => HasRenoDX && (RenoDXUpdateResult?.UpdateAvailable ?? false);
    public string? RenoDXLatestVersion => RenoDXUpdateResult?.LatestVersion;
    public bool IsRenoDXSupported =>
        (CompatibleRenoDXMod is not null ||
         CompatibleRenoDXGenericMod is not null) ||
        (EngineName == Game.Engine.Unity ||
         EngineName == Game.Engine.Unreal);
    public string? ReShadeVersion => _game.ReShade?.Version;
    public string? ReShadeBranch => _game.ReShade?.BranchName.ToString() ?? "Unknown";
    public string? ReShadeArch => _game.ReShade?.Arch.ToString();
    public string? ReShadeFilename => _game.ReShade?.SelectedFileName;

    // Get the actual ReShade object
    internal ReShade? GetReShade() => _game.ReShade;

    // Get the actual RenoDX object
    internal RenoDX? GetRenoDX() => _game.RenoDX;

    // Get the actual game object
    internal Game GetGame() => _game;

    // Call when installing or uninstalling ReShade/RenoDX to notify the VM these have changed since they are derived from _game
    internal void NotifyGameStateChanged()
    {
        OnPropertyChanged(nameof(HasRenoDX));
        OnPropertyChanged(nameof(HasReShade));
        OnPropertyChanged(nameof(CanInstallRenoDX));
        OnPropertyChanged(nameof(CanUpdateReShade));
        OnPropertyChanged(nameof(CanUpdateRenoDX));
        OnPropertyChanged(nameof(ReShadeVersion));
        OnPropertyChanged(nameof(ReShadeLatestVersion));
        OnPropertyChanged(nameof(ReShadeBranch));
        OnPropertyChanged(nameof(ReShadeArch));
        OnPropertyChanged(nameof(ReShadeFilename));
        OnPropertyChanged(nameof(RenoDXName));
        OnPropertyChanged(nameof(RenoDXMaintainer));
        OnPropertyChanged(nameof(RenoDXVersion));
        OnPropertyChanged(nameof(RenoDXLatestVersion));
        OnPropertyChanged(nameof(RenoDXBranch));
        OnPropertyChanged(nameof(RenoDXArch));
    }

    public string? RenoDXName => _game.RenoDX?.SelectedName;
    public string? RenoDXMaintainer => _game.RenoDX?.Maintainer;
    public string? RenoDXVersion => _game.RenoDX?.Version;
    public string? RenoDXBranch => _game.RenoDX?.BranchName.ToString() ?? "Unknown";
    public string? RenoDXArch => _game.RenoDX?.Arch.ToString();

    public bool RenoDXSupportsX64 => CompatibleRenoDXMod?.SupportsX64 ?? false;
    public bool RenoDXSupportsX32 => CompatibleRenoDXMod?.SupportsX32 ?? false;
    public bool RenoDXIsDualArch => CompatibleRenoDXMod?.IsDualArch ?? false;

    public string? RenoDXAddonFileNameX64 => CompatibleRenoDXMod?.AddonFileName64;
    public string? RenoDXAddonFileNameX32 => CompatibleRenoDXMod?.AddonFileName32;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRenoDXInstallArch))]
    [NotifyPropertyChangedFor(nameof(SelectedReShadeInstallArch))]
    [NotifyPropertyChangedFor(nameof(RenoDXWikiDownloadUrl))]
    [NotifyPropertyChangedFor(nameof(RenoDXAddonFileName))]
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

    public string? RenoDXAddonFileName =>
        SelectedRenoDXInstallArch == RenoDX.Architecture.x32
            ? CompatibleRenoDXMod?.AddonFileName32 ?? CompatibleRenoDXMod?.AddonFileName64
            : CompatibleRenoDXMod?.AddonFileName64 ?? CompatibleRenoDXMod?.AddonFileName32;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanInstallRenoDX))]
    [NotifyPropertyChangedFor(nameof(CanUpdateRenoDX))]
    [NotifyPropertyChangedFor(nameof(SelectedRenoDXInstallArch))]
    [NotifyPropertyChangedFor(nameof(SelectedReShadeInstallArch))]
    [NotifyPropertyChangedFor(nameof(RenoDXWikiDownloadUrl))]
    [NotifyPropertyChangedFor(nameof(RenoDXAddonFileName))]
    [NotifyPropertyChangedFor(nameof(RenoDXSupportsX64))]
    [NotifyPropertyChangedFor(nameof(RenoDXSupportsX32))]
    [NotifyPropertyChangedFor(nameof(RenoDXIsDualArch))]
    [NotifyPropertyChangedFor(nameof(RenoDXAddonFileNameX64))]
    [NotifyPropertyChangedFor(nameof(RenoDXAddonFileNameX32))]
    private RenoDXModInfoDto? _compatibleRenoDXMod;

    partial void OnCompatibleRenoDXModChanged(RenoDXModInfoDto? value) => ArchOverride = null;

    [ObservableProperty]
    private RenoDXGenericModInfoDto? _compatibleRenoDXGenericMod;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BannerBitmap))]
    private string? _bannerPathString;

    partial void OnBannerPathStringChanged(string? value) =>
        ResetLazyBitmap(ref _bannerBitmap, value, BannerTargetWidth);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LogoBitmap))]
    private string? _logoPathString;

    partial void OnLogoPathStringChanged(string? value) =>
        ResetLazyBitmap(ref _logoBitmap, value, LogoTargetWidth);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThumbnailBitmap))]
    private string? _thumbnailPathString;

    partial void OnThumbnailPathStringChanged(string? value) =>
        ResetLazyBitmap(ref _thumbnailBitmap, value, ThumbnailTargetWidth);
    
    public Bitmap? BannerBitmap => _bannerBitmap.Value;
    public Bitmap? LogoBitmap => _logoBitmap.Value;
    public Bitmap? ThumbnailBitmap => _thumbnailBitmap.Value;

    private static Lazy<Bitmap?> CreateLazyBitmap(string? path, int targetWidth) =>
        new(() => DecodeBitmap(path, targetWidth), System.Threading.LazyThreadSafetyMode.None);

    private static Bitmap? DecodeBitmap(string? path, int targetWidth)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        using var stream = File.OpenRead(path);

        return Bitmap.DecodeToWidth(stream, targetWidth, BitmapInterpolationMode.HighQuality);
    }

    private void ResetLazyBitmap(ref Lazy<Bitmap?> lazy, string? newPath, int targetWidth)
    {
        if (lazy.IsValueCreated)
            lazy.Value?.Dispose();

        lazy = CreateLazyBitmap(newPath, targetWidth);
    }
}