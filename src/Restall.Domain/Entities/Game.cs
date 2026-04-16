namespace Restall.Domain.Entities;

public sealed class Game
{  
    public enum Platform { Unknown, Steam, Epic, GOG, Ubisoft, EA }
    public enum Engine { Unknown, Unreal, Unity }

    public string? Name { get; init; }
    public Platform PlatformName { get; set; } = Platform.Unknown;
    public Engine EngineName { get; set; } = Engine.Unknown;
    public string? ExecutablePath { get; set; }
    public string? InstallFolder { get; set; }
    public string? PlatformId { get; init; }
    public string? BannerPathString { get; set; }
    public string? LogoPathString { get; set; }
    public string? ThumbnailPathString { get; set; }
    
    public string? GameCoverPathString { get; set; }
    public RenoDX? RenoDX { get; set; }
    public ReShade? ReShade { get; set; }

    public bool HasRenoDX => RenoDX is not null;
    public bool HasReShade => ReShade is not null;
}