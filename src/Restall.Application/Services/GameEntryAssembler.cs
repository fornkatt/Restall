// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Stores;
using Restall.Domain.Entities;

namespace Restall.Application.Services;

public sealed partial class GameEntryAssembler
{
    private readonly ILogger<GameEntryAssembler> _logger;
    private readonly RenoDXCatalog _renoDXCatalog;
    private readonly IUpdateCheckService _updateCheckService;

    public GameEntryAssembler(
        ILogger<GameEntryAssembler> logger,
        RenoDXCatalog renoDXCatalog,
        IUpdateCheckService updateCheckService)
    {
        _logger = logger;
        _renoDXCatalog = renoDXCatalog;
        _updateCheckService = updateCheckService;
    }

    public GameEntry Assemble(Game game)
    {
        var renoDXModDatabase = _renoDXCatalog.ModDatabase;
        var snapshot = _renoDXCatalog.Snapshot;
        var nightlies = _renoDXCatalog.Nightlies;

        var match = RenoDXModMatcher.Match(game.Name, renoDXModDatabase);

        LogRenoDXModMatch(game.Name ?? "Unknown", match);

        var architecture = ArchitectureRecommender.Recommend(game.ReShade, game.RenoDX, match);
        var renoDXEntry = RenoDXEntryBuilder.Build(game.RenoDX, game.EngineName, architecture, match,
            renoDXModDatabase.IsGameModListLoaded, snapshot, nightlies);
        var reShadeUpdate = game.ReShade is null ? null : _updateCheckService.CheckReShadeUpdate(game.ReShade);

        return new GameEntry(game, architecture, renoDXEntry, reShadeUpdate);
    }

    private void LogRenoDXModMatch(string gameName, RenoDXModMatch match)
    {
        switch (match.Kind)
        {
            case RenoDXModMatch.MatchKind.Exact or RenoDXModMatch.MatchKind.Fuzzy:
                LogRenoDXModMatchFound(gameName, match.ModName, match.Kind);
                break;
            case RenoDXModMatch.MatchKind.Tie:
                LogRenoDXModMatchTie(gameName, string.Join(", ", match.TiedNames.Select(n => $"\"{n}\"")));
                break;
            default:
                LogRenoDXModMatchNotFound(gameName);
                break;
        }
    }
}
