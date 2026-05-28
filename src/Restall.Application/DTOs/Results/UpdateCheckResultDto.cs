namespace Restall.Application.DTOs.Results;

public record UpdateCheckResultDto(
    bool UpdateAvailable,
    string? InstalledVersion,
    string? LatestVersion,
    string? ErrorMessage = null
);