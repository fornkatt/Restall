// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.DTOs;

public record DownloadProgressReportDto(
    string Filename,
    int PercentComplete);
