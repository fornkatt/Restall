// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;

namespace Restall.Application.DTOs.Requests;

public record RenoDXInstallRequest(
    Game Game,
    RenoDX.Branch Branch,
    string? NightlyVersion = null,
    GameEntry? GameEntry = null);
