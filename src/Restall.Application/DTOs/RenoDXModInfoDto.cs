namespace Restall.Application.DTOs;

public record RenoDXModInfoDto(
    string Name,
    string? DiscordUrl,
    string? SnapshotUrl64,
    string? SnapshotUrl32,
    string? NexusUrl,
    string? Maintainer,
    string? Notes,
    string? Status,
    string? OverrideAddonFilename = null
)

{
    public string? AddonFilename64 => ExtractFilename(SnapshotUrl64);
    public string? AddonFilename32 => ExtractFilename(SnapshotUrl32);

    public bool SupportsX64 => SnapshotUrl64 is not null;
    public bool SupportsX32 => SnapshotUrl32 is not null;
    public bool IsDualArch => SupportsX64 && SupportsX32;

    public bool HasWikiFilename => AddonFilename64 is not null || AddonFilename32 is not null;

    public RenoDXModSource PreferredManualSource =>
        NexusUrl is not null    ? RenoDXModSource.Nexus :
        DiscordUrl is not null  ? RenoDXModSource.Discord :
                                  RenoDXModSource.Unknown;

    private static string? ExtractFilename(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        ? Path.GetFileName(uri.AbsolutePath)
        : null;
}

public enum RenoDXModSource { Snapshot, Nexus, Discord, Unknown }