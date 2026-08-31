using System.Collections.Immutable;
using Restall.Domain.Entities;

namespace Restall.Application.Helpers;

public sealed record GameCollectionPart(string DisplaySuffix, string FolderSegment);

public sealed record GameCollectionDefinition(
    string Id,
    ImmutableArray<string> NameKeywords,
    ImmutableArray<GameCollectionPart> Parts,
    string ExecutablePathTemplate,
    bool CollapseForModMatching = true
)
{
    public bool Matches(string? name) =>
        !string.IsNullOrWhiteSpace(name) &&
        NameKeywords.All(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));

    public string BuildExecutablePath(Game game, GameCollectionPart part) =>
        Path.Combine(game.InstallFolder!, ExecutablePathTemplate
            .Replace("{part}", part.FolderSegment, StringComparison.Ordinal)
            .Replace('/', Path.DirectorySeparatorChar));
}

public static class GameCollectionCatalog
{
    public static readonly ImmutableArray<GameCollectionDefinition> All =
    [
        new(
            "mass-effect-legendary",
            ["Mass Effect", "Legendary Edition"],
            [
                new("ME1", "ME1"),
                new("ME2", "ME2"),
                new("ME3", "ME3")
            ],
            "Game/{part}"
        ),
        new(
            "shenmue-1-2",
            ["Shenmue I & II"],
            [
                new("I", "sm1"),
                new("II", "sm2")
            ],
            "{part}"
        )
    ];

    public static GameCollectionDefinition? Find(string? name) =>
        All.FirstOrDefault(d => d.Matches(name));
}