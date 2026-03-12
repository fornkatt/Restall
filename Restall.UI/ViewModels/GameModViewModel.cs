using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Application.DTOs;
using Restall.Application.Helpers;
using Restall.Domain.Entities;

namespace Restall.UI.ViewModels;

public partial class GameModViewModel : ObservableObject
{
    private readonly Game _game;

    private Bitmap? _bannerBitmap;
    private Bitmap? _logoBitmap;
    private Bitmap? _thumbnailBitmap;

    public GameModViewModel(Game game)
    {
        _game = game;
        _bannerPathString = game.BannerPathString;
        _logoPathString = game.LogoPathString;
        _thumbnailPathString = game.ThumbnailPathString;
        NormalizedName = GameNameHelper.NormalizeName(game.Name!);

        _bannerBitmap = CreateBitmap(game.BannerPathString);
        _logoBitmap = CreateBitmap(game.LogoPathString);
        _thumbnailBitmap = CreateBitmap(game.ThumbnailPathString);
    }

    private static Bitmap? CreateBitmap(string? path) =>
        !string.IsNullOrWhiteSpace(path) ? new Bitmap(path) : null;

    public string NormalizedName { get; }
    public string? Name => _game.Name;
    public Game.Platform PlatformName => _game.PlatformName;
    public Game.Engine EngineName => _game.EngineName;
    public string? ExecutablePath => _game.ExecutablePath;
    public string? InstallFolder => _game.InstallFolder;
    public bool HasRenoDX => _game.HasRenoDX;
    public bool HasReShade => _game.HasReShade;
    public bool CanInstallRenoDX => CompatibleRenoDXMod is not null || CompatibleRenoDXGenericMod is not null;
    public bool CanUpdateReShade => HasReShade;
    public bool CanUpdateRenoDX => HasRenoDX;

    public string? ReShadeVersion => _game.ReShade?.Version;
    public string? ReShadeBranch => _game.ReShade?.BranchName.ToString() ?? "Unknown";
    public string? ReShadeArch => _game.ReShade?.Arch.ToString();
    public string? ReShadeFileName => _game.ReShade?.SelectedFileName;

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
        OnPropertyChanged(nameof(ReShadeBranch));
        OnPropertyChanged(nameof(ReShadeArch));
        OnPropertyChanged(nameof(ReShadeFileName));
        OnPropertyChanged(nameof(RenoDXName));
        OnPropertyChanged(nameof(RenoDXMaintainer));
        OnPropertyChanged(nameof(RenoDXVersion));
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
    public string? RenoDXWikiDownloadUrlX64 => CompatibleRenoDXMod?.SnapshotUrl64;
    public string? RenoDXWikiDownloadUrlX32 => CompatibleRenoDXMod?.SnapshotUrl32;

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
        ArchOverride == RenoDX.Architecture.x32
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
    [NotifyPropertyChangedFor(nameof(RenoDXWikiDownloadUrlX64))]
    [NotifyPropertyChangedFor(nameof(RenoDXWikiDownloadUrlX32))]
    private RenoDXModInfoDto? _compatibleRenoDXMod;

    partial void OnCompatibleRenoDXModChanged(RenoDXModInfoDto? value) => ArchOverride = null;

    [ObservableProperty]
    private RenoDXGenericModInfoDto? _compatibleRenoDXGenericMod;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BannerBitmap))]
    private string? _bannerPathString;

    partial void OnBannerPathStringChanged(string? value)
    {
        _bannerBitmap?.Dispose();
        _bannerBitmap = CreateBitmap(value);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LogoBitmap))]
    private string? _logoPathString;

    partial void OnLogoPathStringChanged(string? value)
    {
        _logoBitmap?.Dispose();
        _logoBitmap = CreateBitmap(value);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThumbnailBitmap))]
    private string? _thumbnailPathString;

    partial void OnThumbnailPathStringChanged(string? value)
    {
        _thumbnailBitmap?.Dispose();
        _thumbnailBitmap = CreateBitmap(value);
    }
    
    public Bitmap? BannerBitmap => _bannerBitmap;
    public Bitmap? LogoBitmap => _logoBitmap;
    public Bitmap? ThumbnailBitmap => _thumbnailBitmap;

    // Method to sync back manual user changed made in the UI, later feature?
    
    // public Game ToDomain()
    // {
    //     _game.BannerPathString = BannerPathString;
    //     _game.LogoPathString = LogoPathString;
    //     _game.ThumbnailPathString = ThumbnailPathString;
    //     _game.IsInstalled = IsInstalled;
    //     return _game;
    // }
}