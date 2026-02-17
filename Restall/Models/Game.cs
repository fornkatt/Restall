
namespace Restall.Models;

public class Game
{
    
    public required string Name { get; set; }
    public string? PlatformName { get; set; }
    
    public string? ExecutablePath { get; set; }
    public string? InstallFolder { get; set; }
    
    public string? ThumbnailPath { get; set; }
    public string? BannerPath {get; set; }

    public RenoDX? RenoDX { get; set; }
    public ReShade? ReShade { get; set; }

    public bool HasReShade { get; set; } = false;
    public bool HasRenoDX { get; set; } = false;
    public bool IsInstalled { get; set; } = false;


}