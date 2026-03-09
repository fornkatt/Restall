namespace Restall.Application.DTOs;

/// <summary>
/// 
/// For use in ParseService
/// 
///</summary>
public record RenoDXModInfoDto(
    string Name,
    string? DiscordUrl,
    string? SnapshotUrl64,
    string? SnapshotUrl32,
    string? NexusUrl,
    string? Maintainer,
    string? Notes,
    string? Status,
    string? OverrideAddonFileName = null
)

{
    public string? AddonFileName64 => ExtractFileName(SnapshotUrl64);
    public string? AddonFileName32 => ExtractFileName(SnapshotUrl32);
    public string? AddonFileName => AddonFileName64 ?? AddonFileName32 ?? OverrideAddonFileName;

    public bool SupportsX64 => SnapshotUrl64 is not null;
    public bool SupportsX32 => SnapshotUrl32 is not null;
    public bool IsDualArch => SupportsX64 && SupportsX32;

    public bool CanAutoInstall => AddonFileName is not null;

    public RenoDXModSource PreferredManualSource =>
        NexusUrl is not null    ? RenoDXModSource.Nexus :
        DiscordUrl is not null  ? RenoDXModSource.Discord :
                                  RenoDXModSource.Unknown;

    private static string? ExtractFileName(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        ? Path.GetFileName(uri.AbsolutePath)
        : null;
}

public enum RenoDXModSource { Snapshot, Nexus, Discord, Unknown }