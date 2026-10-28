// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.DTOs.Responses;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.Services;
using Restall.Application.Stores;
using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Application.UseCases;

public sealed partial class FullLibraryRefreshUseCase : IFullLibraryRefreshUseCase
{
    private readonly ILogger<FullLibraryRefreshUseCase> _logger;
    private readonly IGameDetectionService _gameDetectionService;
    private readonly IGameArtworkService _gameArtworkService;
    private readonly IModDetectionService _modDetectionService;
    private readonly IVersionCatalog _versionCatalog;
    private readonly IParseService _parseService;
    private readonly IRenoDXModSourceService _renoDXModSourceService;
    private readonly RenoDXModDatabaseBuilder _renoDXModDatabaseBuilder;
    private readonly RenoDXCatalog _renoDXCatalog;
    private readonly GameEntryAssembler _gameEntryAssembler;

    private static readonly ParallelOptions s_artworkParallelOptions = new() { MaxDegreeOfParallelism = 4 };


    public FullLibraryRefreshUseCase(
        ILogger<FullLibraryRefreshUseCase> logger,
        IGameDetectionService gameDetectionService,
        IGameArtworkService gameArtworkService,
        IModDetectionService modDetectionService,
        IVersionCatalog versionCatalog,
        IParseService parseService,
        IRenoDXModSourceService renoDXModSourceService,
        RenoDXModDatabaseBuilder renoDXModDatabaseBuilder,
        RenoDXCatalog renoDXCatalog,
        GameEntryAssembler gameEntryAssembler
    )
    {
        _logger = logger;
        _gameDetectionService = gameDetectionService;
        _gameArtworkService = gameArtworkService;
        _modDetectionService = modDetectionService;
        _versionCatalog = versionCatalog;
        _parseService = parseService;
        _renoDXModSourceService = renoDXModSourceService;
        _renoDXModDatabaseBuilder = renoDXModDatabaseBuilder;
        _renoDXCatalog = renoDXCatalog;
        _gameEntryAssembler = gameEntryAssembler;
    }

    public async Task<RefreshLibraryResponse> ExecuteAsync(
        IProgress<GameScanProgressReportDto>? progress = null)
    {
        UIThreadGuard.AssertNotOnUIThread();

        var gameTask = _gameDetectionService.FindGamesAsync(progress);
        var reShadeVersionsTask = _parseService.FetchReShadeVersionsAsync();
        var renoDXGameModsTask = _renoDXModSourceService.FetchGameModsAsync();
        var renoDXUnrealGenericModsTask = _renoDXModSourceService.FetchUnrealGenericModsAsync();
        var renoDXUnityGenericModsTask = _renoDXModSourceService.FetchUnityGenericModsAsync();
        var renoDXSnapshotTask = _parseService.FetchRenoDXSnapshotAsync();
        var renoDXNightliesTask = _parseService.FetchRenoDXNightlyTagsAsync();


        await Task.WhenAll(gameTask, reShadeVersionsTask, renoDXGameModsTask, renoDXUnrealGenericModsTask,
            renoDXUnityGenericModsTask, renoDXSnapshotTask, renoDXNightliesTask);

        string?[] warnings =
        [
            ApplyFetchedReShadeVersions(reShadeVersionsTask.Result),
            ApplyFetchedRenoDXModDatabase(renoDXGameModsTask.Result, renoDXUnrealGenericModsTask.Result,
                renoDXUnityGenericModsTask.Result),
            ApplyFetchedRenoDXSnapshot(renoDXSnapshotTask.Result),
            ApplyFetchedRenoDXNightlies(renoDXNightliesTask.Result)
        ];

        var gameScanResults = gameTask.Result;
        var games = gameScanResults.Games.OrderBy(g => g.Name);

        return await BuildResultAsync(games, gameScanResults.IsSuccess, gameScanResults.Message,
            [.. warnings.OfType<string>()]);
    }

    private string? ApplyFetchedReShadeVersions(ImmutableArray<string> versions)
    {
        if (versions.IsEmpty)
        {
            LogReShadeVersionsNotFound();
            return GetRefreshFailureMessage("the ReShade version list");
        }

        _versionCatalog.LoadReShadeVersions(versions);

        return null;
    }

    private string? ApplyFetchedRenoDXModDatabase(
        Result<ImmutableArray<RenoDXGameMod>> gameModsResult,
        Result<ImmutableArray<RenoDXUnrealGenericMod>> unrealGenericModsResult,
        Result<ImmutableArray<RenoDXUnityGenericMod>> unityGenericModsResult)
    {
        var lastLoaded = _renoDXCatalog.ModDatabase;

        var databaseResult = _renoDXModDatabaseBuilder.Build(
            GetModsOrLastLoaded(gameModsResult, lastLoaded.GameMods),
            GetModsOrLastLoaded(unrealGenericModsResult, lastLoaded.UnrealGenericMods),
            GetModsOrLastLoaded(unityGenericModsResult, lastLoaded.UnityGenericMods));

        const string modListName = "the RenoDX mod list";

        if (!databaseResult.IsSuccess)
        {
            LogRenoDXModDatabaseBuildFailure(databaseResult.Message);
            return GetRefreshFailureMessage(modListName);
        }

        _renoDXCatalog.LoadModDatabase(databaseResult.Value!);

        if (!gameModsResult.IsSuccess || !unrealGenericModsResult.IsSuccess || !unityGenericModsResult.IsSuccess)
            return GetRefreshFailureMessage(modListName);

        return databaseResult.IsPartial ? GetWarningMessage(databaseResult.WarningType) : null;
    }

    private ImmutableArray<T> GetModsOrLastLoaded<T>(Result<ImmutableArray<T>> modsResult,
        ImmutableArray<T> lastLoadedMods)
    {
        if (modsResult.IsSuccess)
            return modsResult.Value;

        LogRenoDXModDatabaseFileFetchFailure(modsResult.Message, modsResult.Exception);

        return lastLoadedMods;
    }

    private string? ApplyFetchedRenoDXSnapshot(Result<RenoDXTagInfo> snapshotResult)
    {
        if (!snapshotResult.IsSuccess)
        {
            LogRenoDXSnapshotFetchFailure(snapshotResult.Message, snapshotResult.Exception);
            return GetRefreshFailureMessage("the RenoDX Snapshot release");
        }

        _renoDXCatalog.LoadSnapshot(snapshotResult.Value!);

        return snapshotResult.IsPartial ? GetWarningMessage(snapshotResult.WarningType) : null;
    }

    private string? ApplyFetchedRenoDXNightlies(Result<ImmutableArray<RenoDXTagInfo>> nightliesResult)
    {
        if (!nightliesResult.IsSuccess)
        {
            LogRenoDXNightliesFetchFailure(nightliesResult.Message, nightliesResult.Exception);
            return GetRefreshFailureMessage("the RenoDX Nightly releases");
        }

        _renoDXCatalog.LoadNightlies(nightliesResult.Value);

        return nightliesResult.IsPartial ? GetWarningMessage(nightliesResult.WarningType) : null;
    }

    private static string GetRefreshFailureMessage(string listName) =>
        $"Couldn't refresh {listName}. Try refreshing again later.";

    private static string GetWarningMessage(WarningType type) => type switch
    {
        WarningType.RenoDXModDatabaseIncomplete =>
            "Couldn't read the whole RenoDX mod list, so some mods may be missing.",
        WarningType.RenoDXSnapshotFileListUnavailable =>
            "Couldn't read the file list of the RenoDX Snapshot release, so it can't confirm which mods it " +
            "contains.",
        WarningType.RenoDXNightliesIncomplete =>
            "Couldn't load some RenoDX Nightly releases, so some Nightly versions may be missing.",
        _ => "Couldn't load everything during the refresh. Check the log for details."
    };

    private async Task<RefreshLibraryResponse> BuildResultAsync(IOrderedEnumerable<Game> sortedGames, bool success,
        string? errorMessage, ImmutableArray<string> warnings)
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

        return new RefreshLibraryResponse([.. gameEntries], success, warnings, errorMessage);
    }
}
