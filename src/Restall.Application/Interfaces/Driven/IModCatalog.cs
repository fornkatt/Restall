// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

﻿using System.Collections.Immutable;
using Restall.Application.DTOs;

namespace Restall.Application.Interfaces.Driven;

public interface IModCatalog
{
    Task FetchModsAsync();

    ImmutableArray<RenoDXModInfoDto> GetRenoDXWikiMods();
    ImmutableArray<RenoDXGenericModInfoDto> GetRenoDXGenericWikiMods();
    string? GetRenoDXWikiModTypeNotes(RenoDXWikiModType engine);
}