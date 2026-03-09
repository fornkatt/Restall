using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Application.DTOs;
using Restall.Application.Helpers;
using Restall.Domain.Entities;

namespace Restall.UI.ViewModels;

public partial class GameModViewModel : ObservableObject
{
    private readonly Game _game;

    public GameModViewModel(Game game)
    {
        _game = game;
        _bannerPathString = game.BannerPathString;
        _logoPathString = game.LogoPathString;
        _thumbnailPathString = game.ThumbnailPathString;
        NormalizedName = GameNameHelper.NormalizeName(game.Name!);
    }

    public string NormalizedName { get; }
    public string? Name => _game.Name;
    public Game.Platform PlatformName => _game.PlatformName;
    public Game.Engine EngineName => _game.EngineName;
    public string? ExecutablePath => _game.ExecutablePath;
    public string? InstallFolder => _game.InstallFolder;
    public bool HasRenoDX => _game.HasRenoDX;
    public bool HasReShade => _game.HasReShade;
    public bool CanInstallRenoDX => _game.CanInstallRenoDX && CompatibleRenoDXMod is not null;
    public bool CanInstallReShade => _game.CanInstallReShade;
    public bool CanUpdateReShade => _game.CanUpdateReShade;
    public bool CanUpdateRenoDX => _game.CanUpdateRenoDX && CompatibleRenoDXMod is not null;

    public string? ReShadeVersion => _game.ReShade?.Version;
    public ReShade.Branch ReShadeBranch => _game.ReShade?.BranchName ?? ReShade.Branch.Unknown;
    public string? ReShadeFileName => _game.ReShade?.SelectedFileName;

    internal ReShade? GetReShade() => _game.ReShade;
    internal RenoDX? GetRenoDX() => _game.RenoDX;
    internal Game GetGame() => _game;

    public string? RenoDxName => _game.RenoDX?.Name;
    public string? RenoDxVersion => _game.RenoDX?.Version;
    public RenoDX.Branch RenoDxBranch => _game.RenoDX?.BranchName ?? RenoDX.Branch.Unknown;

    [ObservableProperty]
    private RenoDXModInfoDto? _compatibleRenoDXMod;

    [ObservableProperty]
    private RenoDXGenericModInfoDto? _compatibleRenoDXGenericMod;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BannerBitmap))]
    private string? _bannerPathString;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LogoBitmap))]
    private string? _logoPathString;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThumbnailBitmap))]
    private string? _thumbnailPathString;

    [ObservableProperty]
    private bool _isInstalled;
    
    public Bitmap? BannerBitmap =>
        !string.IsNullOrWhiteSpace(BannerPathString) ? new Bitmap(BannerPathString) : null;

    public Bitmap? LogoBitmap =>
        !string.IsNullOrWhiteSpace(LogoPathString) ? new Bitmap(LogoPathString) : null;

    public Bitmap? ThumbnailBitmap =>
        !string.IsNullOrWhiteSpace(ThumbnailPathString) ? new Bitmap(ThumbnailPathString) : null;

    
    
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