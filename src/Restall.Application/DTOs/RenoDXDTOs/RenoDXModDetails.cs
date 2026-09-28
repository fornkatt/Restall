// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXModDetails(
    string? ListedName,
    string? Status,
    string? Author,
    string? Notes,
    string? Method,
    ImmutableArray<string> Upgrades,
    ImmutableArray<RenoDXModLink> Links)
{
    public static readonly RenoDXModDetails Empty = new(null, null, null, null, null,
        [], []);
}
