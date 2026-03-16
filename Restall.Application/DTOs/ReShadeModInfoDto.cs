namespace Restall.Application.DTOs;

public record ReShadeModInfoDto(
    string? Filename,
    string? Version,
    string? StableUrl,
    string? NightlyUrl,
    string? RenoDXUrl,
    string? Notes
    );