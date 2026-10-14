// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXUnityGenericMod(
    string Name,
    RenoDXModStatus Status,
    string? Upgrades,
    string? Comments);
