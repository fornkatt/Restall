namespace Restall.Application.DTOs;

/// <summary>
/// Not used yet. For use in a later implementation when we start working on
/// branches for ReShade
/// </summary>
public record ReShadeModInfoDto(
    string? Filename,
    string? Version,
    string? StableUrl,
    string? NightlyUrl,
    string? RenoDXUrl,
    string? Notes
    );