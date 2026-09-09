// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;

namespace Restall.Application.DTOs.Results;

public record RefreshLibraryResultDto(
    IReadOnlyList<GameInitResultDto> Games,
    bool IsSuccess,
    string? ErrorMessage = null
);

public record GameInitResultDto(
    Game Game,
    RenoDXModInfoDto? CompatibleMod,
    RenoDXGenericModInfoDto? CompatibleGenericMod,
    UpdateCheckResultDto? ReShadeUpdateResult = null,
    UpdateCheckResultDto? RenoDXUpdateResult = null
);