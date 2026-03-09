namespace Restall.Domain.Entities;

public class Game
{  
    public enum Platform { Unknown, Steam, Epic, GOG, Heroic, Ubisoft, EA }
    public enum Engine { Unknown, Unreal, Unity }

    public string? Name { get; init; }
    public Platform PlatformName { get; set; } = Platform.Unknown;
    public Engine EngineName { get; set; } = Engine.Unknown;
    public string? ExecutablePath { get; set; }
    public string? InstallFolder { get; set; }
    public string? PlatformId { get; init; }
    public int? SteamGridDbId { get; init; }
    public string? BannerPathString { get; set; }
    public string? LogoPathString { get; set; }
    public string? ThumbnailPathString { get; set; }
    public RenoDX? RenoDX { get; set; }
    public ReShade? ReShade { get; set; }

    public bool HasRenoDX => RenoDX is not null;
    public bool HasReShade => ReShade is not null;
    public bool CanInstallRenoDX => RenoDX is null && ReShade is not null;
    public bool CanInstallReShade => ReShade is null;
    public bool CanUpdateReShade => HasReShade;
    public bool CanUpdateRenoDX => HasRenoDX;
}