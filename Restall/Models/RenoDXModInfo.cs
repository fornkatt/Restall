namespace Restall.Models;

/// <summary>
/// 
/// For use in ParseService
/// 
///</summary>
public record RenoDXModInfo(
    string Name,
    string? DiscordUrl,
    string? SnapshotUrl,
    string? NexusUrl,
    string? Maintainer,
    string? Notes,
    string? Status
);

public enum RenoDXModSource { Snapshot, Nexus, Discord, Unknown }