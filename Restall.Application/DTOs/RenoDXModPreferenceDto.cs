namespace Restall.Application.DTOs;

/// <summary>
/// For use in later implementations to swap branches and sources of the mod between a wider selection than currently
/// available.
/// </summary>
public record RenoDXModPreferenceDto(
    string Name,
    RenoDXModSource PreferredSource,
    string? LastSeenVersion = null,
    DateTimeOffset? LastUpdated = null,
    DateOnly? LastChecked = null
    );