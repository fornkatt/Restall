// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

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
        IProgress<GameScanProgressReportDto>? progress = null) =>
        BuildResultAsync(existingGames.OrderBy(g => g.Name), true, null);

    private async Task<RefreshLibraryResponse> BuildResultAsync(IOrderedEnumerable<Game> sortedGames, bool success,
        string? errorMessage)
    {
        HashSet<Task> artworkTasks = [];
        List<GameEntry> gameEntries = [];

        foreach (var game in sortedGames)
        {
            if (string.IsNullOrWhiteSpace(game.Name))
                continue;

            var reShade = await _modDetectionService.DetectInstalledReShadeAsync(game.ExecutablePath!);
            var renoDx = await _modDetectionService.DetectInstalledRenoDXAsync(game.ExecutablePath!);

            // TODO(): handle multiple mods found with user choice
            game.ReShade = reShade.Value?.FirstOrDefault();
            game.RenoDX = renoDx.Value?.FirstOrDefault();

            artworkTasks.Add(_gameArtworkService.EnrichGameArtworkAsync(game));

            gameEntries.Add(_gameEntryAssembler.Assemble(game));
        }

        await Task.WhenAll(artworkTasks);

        return new RefreshLibraryResponse([.. gameEntries], success, [], errorMessage);
    }
}
