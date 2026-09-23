// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.DTOs;

public record GameScanProgressReportDto(
    string CompletedPlatform,
    int ScannersCompleted,
    int TotalScanners,
    bool IsSuccess,
    string? Message = null);
