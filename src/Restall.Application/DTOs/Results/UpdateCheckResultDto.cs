namespace Restall.Application.DTOs.Results;

// TODO: Error message in DTO? Propagate through Result instead
public record UpdateCheckResultDto(
    bool UpdateAvailable,
    string? InstalledVersion,
    string? LatestVersion,
    string? ErrorMessage = null
);