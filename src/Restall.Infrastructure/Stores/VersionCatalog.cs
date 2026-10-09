// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Stores;
using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Infrastructure.Stores;

internal sealed class VersionCatalog : IVersionCatalog
{
    private readonly RenoDXCatalog _renoDXCatalog;

    private ImmutableDictionary<ReShade.Branch, ImmutableArray<string>> _reShadeVersions = [];

    public VersionCatalog(
        RenoDXCatalog renoDXCatalog
    )
    {
        _renoDXCatalog = renoDXCatalog;
    }

    public void LoadReShadeVersions(ImmutableArray<string> versions) =>
        _reShadeVersions = ImmutableDictionary<ReShade.Branch, ImmutableArray<string>>.Empty
            .Add(ReShade.Branch.Stable, versions);

    public string? GetLatestReShadeVersion(ReShade.Branch branch)
    {
        if (!_reShadeVersions.TryGetValue(branch, out var versions) || versions.Length == 0)
            return null;

        return versions[0];
    }

    public ImmutableArray<string> GetAvailableReShadeVersions(ReShade.Branch branch) =>
        _reShadeVersions.TryGetValue(branch, out var versions) ? versions : [];

    // TODO: delete all below later after everything is wired up
    public RenoDXTagInfo? GetLatestRenoDXVersionByTag(RenoDX.Branch branch) => branch switch
    {
        RenoDX.Branch.Snapshot => _renoDXCatalog.Snapshot,
        RenoDX.Branch.Nightly => _renoDXCatalog.Nightlies.MaxBy(n => n.Date),
        _ => null
    };

    public ImmutableArray<RenoDXTagInfo> GetAllRenoDXNightlies() => _renoDXCatalog.Nightlies;
}
