// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Infrastructure.Stores;

internal sealed class VersionCatalog : IVersionCatalog
{
    private ImmutableDictionary<ReShade.Branch, ImmutableArray<string>> _reShadeVersions = [];

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
}
