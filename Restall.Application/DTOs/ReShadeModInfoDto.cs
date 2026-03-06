namespace Restall.Application.DTOs;

public record ReShadeModInfoDto(
    string? FileName,
    string? Version,
    string? StableUrl,
    string? NightlyUrl,
    string? RenoDXUrl,
    string? Notes
    );