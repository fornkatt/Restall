// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXGenericAddonInfo(
    string Name,
    string Author,
    string Notes,
    string? EngineIni);
