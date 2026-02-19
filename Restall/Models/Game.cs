using System.Threading.Tasks;
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

    private string? _thumbnailPath;
    private string? _bannerPath;
    private string? _logoPath;
    
    public string? RenoDXVersion => HasRenoDX ? "2026-02-18" : "Not installed";
    public string? ReShadeVersion => HasReShade ? "6.7.2" : "Not installed";

    private bool _hasReShade;
    private bool _hasRenoDX;
    private bool _isInstalled;
    

    public string? LogoPath
    {
        get => _logoPath;
        set => SetProperty(ref _logoPath, value);
    }
    
    public string? ThumbnailPath
    {
        get => _thumbnailPath;
        set => SetProperty(ref _thumbnailPath, value);
    }

    public string? BannerPath
    {
        get => _bannerPath;
        set => SetProperty(ref _bannerPath, value);
    }
    
    public bool HasReShade
    {
        get => _hasReShade;
        set => SetProperty(ref _hasReShade, value);
    }
    
    public bool HasRenoDX
    {
        get => _hasRenoDX;
        set => SetProperty(ref _hasRenoDX, value);
    }
    
    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetProperty(ref _isInstalled, value);
    }
}