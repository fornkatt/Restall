namespace Restall.Application.DTOs;

/// <summary>
/// 
/// For use in ParseService
/// 
///</summary>
public record RenoDXModInfoDto(
    string Name,
    string? DiscordUrl,
    string? SnapshotUrl,
    string? NexusUrl,
    string? Maintainer,
    string? Notes,
    string? Status
);

public enum RenoDXModSource { Snapshot, Nexus, Discord, Unknown }