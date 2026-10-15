// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Application.Tests.Fakes;

public sealed class FakeGameDetectionService : IGameDetectionService
{
    private GameScanResultDto? _scanResult;

    public void ReturnGames(params Game[] games) =>
        _scanResult = new GameScanResultDto(Game.Platform.Unknown, games, true);

    public void ReturnScanFailure(string message) =>
        _scanResult = new GameScanResultDto(Game.Platform.Unknown, [], false, message);

    public Task<GameScanResultDto> FindGamesAsync(IProgress<GameScanProgressReportDto>? progress = null) =>
        Task.FromResult(_scanResult ?? throw new InvalidOperationException("No fake scan result is set up"));
}
