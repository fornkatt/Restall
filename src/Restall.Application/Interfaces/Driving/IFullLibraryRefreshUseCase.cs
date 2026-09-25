// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;

namespace Restall.Application.Interfaces.Driving;

public interface IFullLibraryRefreshUseCase
{
    Task<RefreshLibraryResultDto> ExecuteAsync(IProgress<GameScanProgressReportDto>? progress = null);
}
