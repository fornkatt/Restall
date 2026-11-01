// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using System.Collections.Immutable;

namespace Restall.Application.Interfaces.Driven;

public interface IRenoDXModSourceService
{
    /// <summary>
    /// Fetches supported RenoDX mods from a JSON database.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ErrorType.DownloadFailed"/>
    /// <br/>
    /// <see cref="ErrorType.NetworkTimeout"/>
    /// <br/>
    /// <see cref="ErrorType.JsonParseFailure"/>
    /// <br/>
    /// <see cref="ErrorType.JsonArrayNotFound"/>
    /// </para>
    /// </summary>
    Task<Result<ImmutableArray<RenoDXGameMod>>> FetchGameModsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches known supported RenoDX Unreal generic mods from a JSON database.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ErrorType.DownloadFailed"/>
    /// <br/>
    /// <see cref="ErrorType.NetworkTimeout"/>
    /// <br/>
    /// <see cref="ErrorType.JsonParseFailure"/>
    /// <br/>
    /// <see cref="ErrorType.JsonArrayNotFound"/>
    /// </para>
    /// </summary>
    Task<Result<ImmutableArray<RenoDXUnrealGenericMod>>> FetchUnrealGenericModsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches known supported RenoDX Unity generic mods from a JSON database.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ErrorType.DownloadFailed"/>
    /// <br/>
    /// <see cref="ErrorType.NetworkTimeout"/>
    /// <br/>
    /// <see cref="ErrorType.JsonParseFailure"/>
    /// <br/>
    /// <see cref="ErrorType.JsonArrayNotFound"/>
    /// </para>
    /// </summary>
    Task<Result<ImmutableArray<RenoDXUnityGenericMod>>> FetchUnityGenericModsAsync(
        CancellationToken cancellationToken = default);
}
