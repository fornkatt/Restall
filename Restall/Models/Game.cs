using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
// ReSharper disable InconsistentNaming

namespace Restall.Models;


public class Game : ObservableObject
{  
    // PLATFORM
    public enum Platform
    {
        Unknown,
        EA,
        Steam,
        GOG,
        Epic,
        Ubisoft
    }
    
    //ENGINE
    public enum Engine
    {
        Unknown,
        Unreal,
        Unity
    }
    
    public string? Name { get; init; }
    
    //ENUM
    public Platform PlatformName { get; set; } = Platform.Unknown;
    public Engine EngineName { get; set; } = Engine.Unknown;
    
    
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
    
    //IF THE GAME IS INSTALLED?
    private bool _isInstalled;
    
    private bool _hasReShade;
    private bool _canInstallReShade;
    private bool _canUpdateReShade; 
    private bool _hasRenoDX;
    private bool _canInstallRenoDX;
    private bool _canUpdateRenoDX;
    
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
    
    // KOLLA AVAILABLE VERSION. KONVERTERA MED IVALUECONVERTER
    public bool CanUpdateReShade => HasReShade;
    public bool CanUpdateRenoDX => HasRenoDX;
    
    //TODO: MANUAL BUTTON THAT DISABLES UPDATE AVAILABLE
    
    //IF THE GAME IS INSTALLED
    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }
}