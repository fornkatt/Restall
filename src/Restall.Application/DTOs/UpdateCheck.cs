// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.DTOs;

public sealed record UpdateCheck(
    bool UpdateAvailable,
    string? InstalledVersion,
    string? LatestVersion);
