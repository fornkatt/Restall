// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

﻿using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;

namespace Restall.Application.Interfaces.Driven;

public interface IGameDetectionService
{
    Task<GameScanResultDto> FindGamesAsync(IProgress<GameScanProgressReportDto>? progress = null);
}