// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Infrastructure.DTOs;

internal sealed record PeVersionInfo(
    string? OriginalFilename,
    string? FileVersion,
    string? ProductName,
    string? ProductVersion);
