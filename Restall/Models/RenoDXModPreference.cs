using System;

namespace Restall.Models;

public record RenoDXModPreference(
    string Name,
    RenoDXModSource PreferredSource,
    string? LastSeenVersion = null,
    DateTimeOffset? LastUpdated = null,
    DateOnly? LastChecked = null
);