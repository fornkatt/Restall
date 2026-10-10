// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.DTOs.Responses;

namespace Restall.Application.Interfaces.Driving;

public interface IFullLibraryRefreshUseCase
{
    Task<RefreshLibraryResponse> ExecuteAsync(IProgress<GameScanProgressReportDto>? progress = null);
}
