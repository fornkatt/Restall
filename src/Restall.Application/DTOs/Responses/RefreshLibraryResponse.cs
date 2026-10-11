// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;

namespace Restall.Application.DTOs.Responses;

public record RefreshLibraryResponse(
    ImmutableArray<GameEntry> GameEntries,
    bool IsSuccess,
    ImmutableArray<string> Warnings,
    string? ErrorMessage = null);
