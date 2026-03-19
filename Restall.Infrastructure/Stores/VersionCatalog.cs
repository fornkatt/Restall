using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Stores;

internal sealed class VersionCatalog : IVersionCatalog
{
    private readonly IParseService _parseService;
    private readonly ILogService _logService;

    private readonly Dictionary<ReShade.Branch, IReadOnlyList<string>> _reShadeVersions = [];
    private readonly Dictionary<RenoDX.Branch, IReadOnlyList<RenoDXTagInfoDto>> _renoDXTags = [];

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

        _reShadeVersions[ReShade.Branch.Stable] = reShadeVersionsTask.Result;

        if (renoDXSnapshotTask.Result is not null)
            _renoDXTags[RenoDX.Branch.Snapshot] = [renoDXSnapshotTask.Result];

        _renoDXTags[RenoDX.Branch.Nightly] = [..renoDXNightlyTask.Result];

        await _logService.LogInfoAsync($"Version catalog populated. " +
            $"ReShade {reShadeVersionsTask.Result.Count} versions. " +
            $"RenoDX Snapshot: {(renoDXSnapshotTask.Result is not null ? renoDXSnapshotTask.Result.Version : "none")}. " +
            $"RenoDX Nightly: {renoDXNightlyTask.Result.Count} tags.");
    }

    public string? GetLatestReShadeVersion(ReShade.Branch branch)
    {
        if (!_reShadeVersions.TryGetValue(branch, out var versions) || versions.Count == 0)
            return null;

        return versions[0];
    }

    public IReadOnlyList<string> GetAvailableReShadeVersions(ReShade.Branch branch) =>
        _reShadeVersions.TryGetValue(branch, out var versions) ? versions : [];

    public RenoDXTagInfoDto? GetLatestRenoDXVersionByTag(RenoDX.Branch branch)
    {
        if (!_renoDXTags.TryGetValue(branch, out var versions) || versions.Count == 0)
            return null;

        return versions[0];
    }

    public IReadOnlyList<RenoDXTagInfoDto> GetAllRenoDXNightlies() =>
        _renoDXTags.TryGetValue(RenoDX.Branch.Nightly, out var versions) ? versions : [];
}