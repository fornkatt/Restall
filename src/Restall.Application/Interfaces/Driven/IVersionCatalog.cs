// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Application.Interfaces.Driven;

public interface IVersionCatalog
{
    void LoadReShadeVersions(ImmutableArray<string> versions);
    string? GetLatestReShadeVersion(ReShade.Branch branch);
    ImmutableArray<string> GetAvailableReShadeVersions(ReShade.Branch branch);

    RenoDXTagInfo? GetLatestRenoDXVersionByTag(RenoDX.Branch branch);
    ImmutableArray<RenoDXTagInfo> GetAllRenoDXNightlies();
}
