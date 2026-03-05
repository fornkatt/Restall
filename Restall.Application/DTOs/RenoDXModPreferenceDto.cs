namespace Restall.Application.DTOs;

public record RenoDXModPreferenceDto(
    string Name,
    RenoDXModSource PreferredSource,
    string? LastSeenVersion = null,
    DateTimeOffset? LastUpdated = null,
    DateOnly? LastChecked = null
);