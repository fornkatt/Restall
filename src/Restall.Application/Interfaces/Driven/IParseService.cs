// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.DTOs.RenoDXDTOs;
using System.Collections.Immutable;

namespace Restall.Application.Interfaces.Driven;

public interface IParseService
{
    Task<ImmutableArray<string>> FetchReShadeVersionsAsync();

    Task<Result<RenoDXTagInfo>> FetchRenoDXSnapshotAsync(CancellationToken cancellationToken = default);
    Task<Result<ImmutableArray<RenoDXTagInfo>>> FetchRenoDXNightlyTagsAsync(
        CancellationToken cancellationToken = default);
}
