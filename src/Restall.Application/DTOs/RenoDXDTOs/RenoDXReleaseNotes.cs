// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXReleaseNotes(
    string Version,
    bool IsInstalled,
    bool IsLatest,
    ImmutableArray<string> Lines);
