// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;

namespace Restall.Application.DTOs.Results;

/// <summary>
/// Not used yet. For a future implementation to prompt for a deep scan of mods under certain conditions.
/// </summary>
/// <param name="ShouldPromptForDeepScan"></param>
public record ModOperationResultDto(
    bool IsSuccess,
    Game UpdatedGame,
    string? Message = null,
    bool ShouldPromptForDeepScan = false,
    UpdateCheckResultDto? UpdateCheckResult = null);
