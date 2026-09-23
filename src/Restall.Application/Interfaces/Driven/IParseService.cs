// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using System.Collections.Immutable;

namespace Restall.Application.Interfaces.Driven;

public interface IParseService
{
    Task<ImmutableArray<string>> FetchReShadeVersionsAsync();

    Task<RenoDXWikiParseResultDto> FetchRenoDXWikiModsAsync();
    Task<RenoDXTagInfoDto?> FetchRenoDXSnapshotAsync();
    Task<ImmutableArray<RenoDXTagInfoDto>> FetchRenoDXNightlyTagsAsync();
}
