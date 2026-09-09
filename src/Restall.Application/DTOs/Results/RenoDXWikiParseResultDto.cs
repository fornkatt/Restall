// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;

namespace Restall.Application.DTOs.Results;

public record RenoDXWikiParseResultDto(
    ImmutableArray<RenoDXModInfoDto> WikiMods,
    ImmutableArray<RenoDXGenericModInfoDto> GenericWikiMods,
    ImmutableDictionary<RenoDXWikiModType, string> EngineNotes);