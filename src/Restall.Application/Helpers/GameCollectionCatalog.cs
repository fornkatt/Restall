using System.Collections.Immutable;
using Restall.Domain.Entities;

namespace Restall.Application.Helpers;

public sealed record GameCollectionDefinition(
    string Id,
    ImmutableArray<string> NameKeywords,
    ImmutableArray<string> PartSuffixes,
    string ExecutablePathTemplate,
    bool CollapseForModMatching = true
)
{
    public bool Matches(string? name) =>
        !string.IsNullOrWhiteSpace(name) &&
        NameKeywords.All(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));

    public string BuildExecutablePath(Game game, string part) =>
        Path.Combine(game.InstallFolder!, ExecutablePathTemplate
            .Replace("{part}", part, StringComparison.OrdinalIgnoreCase)
            .Replace('/', Path.DirectorySeparatorChar));
}

public static class GameCollectionCatalog
{
    public static readonly ImmutableArray<GameCollectionDefinition> All =
    [
        new GameCollectionDefinition(
            "mass-effect-legendary",
            ["Mass Effect", "Legendary Edition"],
            ["ME1", "ME2", "ME3"],
            "Game/{part}"
        )
    ];

    public static GameCollectionDefinition? Find(string? name) =>
        All.FirstOrDefault(d => d.Matches(name));
}