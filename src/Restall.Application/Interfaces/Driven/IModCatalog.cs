// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using System.Collections.Immutable;

namespace Restall.Application.Interfaces.Driven;

public interface IModCatalog
{
    Task FetchModsAsync();

    ImmutableArray<RenoDXModInfoDto> GetRenoDXWikiMods();
    ImmutableArray<RenoDXGenericModInfoDto> GetRenoDXGenericWikiMods();
    string? GetRenoDXWikiModTypeNotes(RenoDXWikiModType engine);
}
