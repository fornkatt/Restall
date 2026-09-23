// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Application.Interfaces.Driven;

public interface IVersionCatalog
{
    Task FetchVersionsAsync();

    string? GetLatestReShadeVersion(ReShade.Branch branch);
    ImmutableArray<string> GetAvailableReShadeVersions(ReShade.Branch branch);

    RenoDXTagInfoDto? GetLatestRenoDXVersionByTag(RenoDX.Branch branch);
    ImmutableArray<RenoDXTagInfoDto> GetAllRenoDXNightlies();
}
