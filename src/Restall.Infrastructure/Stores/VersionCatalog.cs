using System.Collections.Immutable;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Stores;

internal sealed class VersionCatalog : IVersionCatalog
{
    private readonly IParseService _parseService;
    private readonly ILogService _logService;

    private ImmutableDictionary<ReShade.Branch, ImmutableArray<string>> _reShadeVersions = ImmutableDictionary<ReShade.Branch, ImmutableArray<string>>.Empty;
    private ImmutableDictionary<RenoDX.Branch, ImmutableArray<RenoDXTagInfoDto>> _renoDXTags = ImmutableDictionary<RenoDX.Branch, ImmutableArray<RenoDXTagInfoDto>>.Empty;

    public VersionCatalog(
        IParseService parseService,
        ILogService logService
        )
    {
        _parseService = parseService;
        _logService = logService;
    }

    public async Task FetchVersionsAsync()
    {
        var reShadeVersionsTask = _parseService.FetchReShadeVersionsAsync();
        var renoDXSnapshotTask = _parseService.FetchRenoDXSnapshotAsync();
        var renoDXNightlyTask = _parseService.FetchRenoDXNightlyTagsAsync();

        await Task.WhenAll(reShadeVersionsTask, renoDXSnapshotTask, renoDXNightlyTask);

        _reShadeVersions = ImmutableDictionary<ReShade.Branch, ImmutableArray<string>>.Empty
            .Add(ReShade.Branch.Stable, reShadeVersionsTask.Result);

        var renoDXBuilder = ImmutableDictionary.CreateBuilder<RenoDX.Branch, ImmutableArray<RenoDXTagInfoDto>>();
        if (renoDXSnapshotTask.Result is not null)
            renoDXBuilder[RenoDX.Branch.Snapshot] = [renoDXSnapshotTask.Result];
        
        renoDXBuilder[RenoDX.Branch.Nightly] = renoDXNightlyTask.Result;
        _renoDXTags = renoDXBuilder.ToImmutable();

        await _logService.LogInfoAsync($"Version catalog populated. " +
            $"ReShade {reShadeVersionsTask.Result.Length} versions. " +
            $"RenoDX Snapshot: {(renoDXSnapshotTask.Result is not null ? renoDXSnapshotTask.Result.Version : "none")}. " +
            $"RenoDX Nightly: {renoDXNightlyTask.Result.Length} tags.");
    }

    public string? GetLatestReShadeVersion(ReShade.Branch branch)
    {
        if (!_reShadeVersions.TryGetValue(branch, out var versions) || versions.Length == 0)
            return null;

        return versions[0];
    }

    public ImmutableArray<string> GetAvailableReShadeVersions(ReShade.Branch branch) =>
        _reShadeVersions.TryGetValue(branch, out var versions) ? versions : [];

    public RenoDXTagInfoDto? GetLatestRenoDXVersionByTag(RenoDX.Branch branch)
    {
        if (!_renoDXTags.TryGetValue(branch, out var versions) || versions.Length == 0)
            return null;

        return versions[0];
    }

    public ImmutableArray<RenoDXTagInfoDto> GetAllRenoDXNightlies() =>
        _renoDXTags.TryGetValue(RenoDX.Branch.Nightly, out var versions) ? versions : [];
}