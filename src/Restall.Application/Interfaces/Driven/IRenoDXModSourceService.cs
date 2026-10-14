// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.DTOs.RenoDXDTOs;
using System.Collections.Immutable;

namespace Restall.Application.Interfaces.Driven;

public interface IRenoDXModSourceService
{
    Task<Result<ImmutableArray<RenoDXGameMod>>> FetchGameModsAsync(CancellationToken cancellationToken = default);
    Task<Result<ImmutableArray<RenoDXUnrealGenericMod>>> FetchUnrealGenericModsAsync(
        CancellationToken cancellationToken = default);
    Task<Result<ImmutableArray<RenoDXUnityGenericMod>>> FetchUnityGenericModsAsync(
        CancellationToken cancellationToken = default);
}
