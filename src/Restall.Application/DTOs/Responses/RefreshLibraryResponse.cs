// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Application.DTOs.Responses;

public record RefreshLibraryResponse(
    IReadOnlyList<GameInitResultDto> Games,
    bool IsSuccess,
    ImmutableArray<string> Warnings,
    string? ErrorMessage = null);

public record GameInitResultDto(
    Game Game,
    RenoDXModInfoDto? CompatibleMod,
    RenoDXGenericModInfoDto? CompatibleGenericMod,
    UpdateAvailability? ReShadeUpdateResult = null,
    UpdateAvailability? RenoDXUpdateResult = null,
    GameEntry? Entry = null);
