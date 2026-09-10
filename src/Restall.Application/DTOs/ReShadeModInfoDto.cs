// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.DTOs;

/// <summary>
/// Not used yet. For use in a later implementation when we start working on
/// branches for ReShade
/// </summary>
public record ReShadeModInfoDto(
    string? Filename,
    string? Version,
    string? StableUrl,
    string? NightlyUrl,
    string? RenoDXUrl,
    string? Notes);
