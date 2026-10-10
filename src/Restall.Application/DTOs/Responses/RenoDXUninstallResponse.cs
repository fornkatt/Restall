// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;

namespace Restall.Application.DTOs.Responses;

public sealed record RenoDXUninstallResponse(
    bool IsSuccess,
    Game Game,
    GameEntry? GameEntry = null,
    string? Message = null);
