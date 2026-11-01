// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.DTOs;
using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.Services;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public sealed class GameRefreshUseCase : IGameRefreshUseCase
{
    private readonly IGameArtworkService _gameArtworkService;
    private readonly IModDetectionService _modDetectionService;
    private readonly GameEntryAssembler _gameEntryAssembler;

    private static readonly ParallelOptions s_artworkParallelOptions = new() { MaxDegreeOfParallelism = 4 };

    public GameRefreshUseCase(
        IGameArtworkService gameArtworkService,
        IModDetectionService modDetectionService,
        GameEntryAssembler gameEntryAssembler
    )
    {
        _gameArtworkService = gameArtworkService;
        _modDetectionService = modDetectionService;
        _gameEntryAssembler = gameEntryAssembler;
    }

    public Task<RefreshLibraryResponse> ExecuteAsync(IReadOnlyList<Game> existingGames,
        IProgress<GameScanProgressReportDto>? progress = null)
    {
        UIThreadGuard.AssertNotOnUIThread();

        return BuildResultAsync(existingGames.OrderBy(g => g.Name), true, null);
    }

    private async Task<RefreshLibraryResponse> BuildResultAsync(IOrderedEnumerable<Game> sortedGames, bool success,
        string? errorMessage)
    {
        var namedGames = sortedGames.Where(game => !string.IsNullOrWhiteSpace(game.Name)).ToList();
        var artworkTask = Parallel.ForEachAsync(namedGames, s_artworkParallelOptions,
            (game, _) => new ValueTask(_gameArtworkService.EnrichGameArtworkAsync(game)));
        var gameEntries = new List<GameEntry>();

        foreach (var game in namedGames)
        {
            var reShade = await _modDetectionService.DetectInstalledReShadeAsync(game.ExecutablePath!);
            var renoDX = await _modDetectionService.DetectInstalledRenoDXAsync(game.ExecutablePath!);

            // TODO: handle multiple mods found with user choice
            game.ReShade = reShade.Value?.FirstOrDefault();
            game.RenoDX = renoDX.Value?.FirstOrDefault();

            gameEntries.Add(_gameEntryAssembler.Assemble(game));
        }

        await artworkTask;

        return new RefreshLibraryResponse([.. gameEntries], success, [], errorMessage);
    }
}
