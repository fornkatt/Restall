// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Application.Tests.Fakes;

public sealed class FakeVersionCatalog : IVersionCatalog
{
    public void LoadReShadeVersions(ImmutableArray<string> versions)
    {
    }

    public string? GetLatestReShadeVersion(ReShade.Branch branch) =>
        throw new NotSupportedException("The library refresh doesn't read ReShade versions");

    public ImmutableArray<string> GetAvailableReShadeVersions(ReShade.Branch branch) =>
        throw new NotSupportedException("The library refresh doesn't read ReShade versions");
}
