using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
// ReSharper disable InconsistentNaming

namespace Restall.Models;

public class Game : ObservableObject
{
    public string? Name { get; init; }
    public string? PlatformName { get; init; }

    public string? ExecutableName { get; set; }
    public string? ExecutablePath { get; set; }
    public string? InstallFolder { get; set; }
    public int? SteamGridDbId { get; }
    
    private string? _bannerPath;
    private string? _logoPath;
    private string? _thumbnailPath;
    
    //public string? RenoDXVersion => HasRenoDX ? "2026-02-18" : "Not installed";
    //public string? ReShadeVersion => HasReShade ? "6.7.2" : "Not installed";

    public RenoDX? RenoDX { get; set; }
    public ReShade? ReShade { get; set; }
    
    
    private bool _hasReShade;
    private bool _canInstallReShade;
    private bool _canUpdateReShade; 
    private bool _hasRenoDX;
    private bool _canInstallRenoDX;
    private bool _canUpdateRenoDX;
    private bool _isInstalled;
    
    public string? BannerPathString
    {
        get => _bannerPath;
        set => SetProperty(ref _bannerPath, value);
    }
    public Bitmap? BannerPath => !string.IsNullOrWhiteSpace(_bannerPath) ? new Bitmap(_bannerPath) : null;

    public string? LogoPathString
    {
        get => _logoPath;
        set => SetProperty(ref _logoPath, value);
    }
    public Bitmap? LogoPath => !string.IsNullOrWhiteSpace(_logoPath) ? new Bitmap(_logoPath) : null;

    public string? ThumbnailPathString
    {
        get => _thumbnailPath;
        set => SetProperty(ref _thumbnailPath, value);
    }
    public Bitmap? ThumbnailPath => !string.IsNullOrWhiteSpace(_thumbnailPath) ? new Bitmap(_thumbnailPath) : null;

    public bool HasRenoDX => RenoDX != null;
    public bool HasReShade => ReShade != null;
    public bool CanInstallRenoDX => RenoDX == null;
    public bool CanInstallReShade => ReShade == null;
    // KOLLA AVAILABLE VERSION. KONVERTERA 
    public bool CanUpdateReShade => _hasReShade;
    public bool CanUpdateRenoDX => _hasRenoDX;
    
    //TODO: MANUAL BUTTON THAT DISABLES UPDATE AVAILABLE
    
    
    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }
}