namespace Restall.Application.DTOs;

public record UpdateCheckResultDto(
    bool UpdateAvailable,
    string? InstalledVersion,
    string? LatestVersion,
    string? ErrorMessage = null
    );