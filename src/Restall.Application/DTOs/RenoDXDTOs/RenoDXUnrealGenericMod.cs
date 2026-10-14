// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXUnrealGenericMod(
    string Name,
    RenoDXModStatus Status,
    RenoDXUnrealGenericMod.UnrealModMethod Method,
    string? Upgrades,
    string? Comments)
{
    public enum UnrealModMethod { Native, Upgrade, Ini }
}
