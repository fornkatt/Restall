// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;

namespace Restall.Application.DTOs;

public sealed record GameScanResultDto(
    Game.Platform Platform,
    IReadOnlyList<Game> Games,
    bool IsSuccess,
    string? Message = null);
