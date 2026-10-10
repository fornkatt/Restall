// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.DTOs.Responses;
using Restall.Application.Helpers;
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
    private readonly IUpdateCheckService _updateCheckService;
    private readonly IVersionCatalog _versionCatalog;
    private readonly IModCatalog _modCatalog;
    private readonly IParseService _parseService;
    private readonly IRenoDXModSourceService _renoDXModSourceService;
    private readonly RenoDXModDatabaseBuilder _renoDXModDatabaseBuilder;
    private readonly RenoDXCatalog _renoDXCatalog;
    private readonly GameEntryAssembler _gameEntryAssembler;


    public FullLibraryRefreshUseCase(
        ILogger<FullLibraryRefreshUseCase> logger,
        IGameDetectionService gameDetectionService,
        IGameArtworkService gameArtworkService,
        IModDetectionService modDetectionService,
        IUpdateCheckService updateCheckService,
        IVersionCatalog versionCatalog,
        IModCatalog modCatalog,
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
        _updateCheckService = updateCheckService;
        _versionCatalog = versionCatalog;
        _modCatalog = modCatalog;
        _parseService = parseService;
        _renoDXModSourceService = renoDXModSourceService;
        _renoDXModDatabaseBuilder = renoDXModDatabaseBuilder;
        _renoDXCatalog = renoDXCatalog;
        _gameEntryAssembler = gameEntryAssembler;
    }

    public async Task<RefreshLibraryResponse> ExecuteAsync(
        IProgress<GameScanProgressReportDto>? progress = null)
    {
        var gameTask = _gameDetectionService.FindGamesAsync(progress);
        var reShadeVersionsTask = _parseService.FetchReShadeVersionsAsync();
        var renoDXGameModsTask = _renoDXModSourceService.FetchGameModsAsync();
        var renoDXUnrealGenericModsTask = _renoDXModSourceService.FetchUnrealGenericModsAsync();
        var renoDXUnityGenericModsTask = _renoDXModSourceService.FetchUnityGenericModsAsync();
        var renoDXSnapshotTask = _parseService.FetchRenoDXSnapshotAsync();
        var renoDXNightliesTask = _parseService.FetchRenoDXNightlyTagsAsync();

        // TODO: remove later
        var wikiTask = _modCatalog.FetchModsAsync();

        await Task.WhenAll(gameTask, reShadeVersionsTask, renoDXGameModsTask, renoDXUnrealGenericModsTask,
            renoDXUnityGenericModsTask, renoDXSnapshotTask, renoDXNightliesTask, wikiTask);

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

        if (databaseResult.IsSuccess)
            _renoDXCatalog.LoadModDatabase(databaseResult.Value!);
        else
            LogRenoDXModDatabaseBuildFailure(databaseResult.Message);

        if (!databaseResult.IsSuccess
            || !gameModsResult.IsSuccess
            || !unrealGenericModsResult.IsSuccess
            || !unityGenericModsResult.IsSuccess)
            return GetRefreshFailureMessage("the RenoDX mod list");

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
        HashSet<Task> artworkTasks = [];
        List<GameInitResultDto> results = [];

        foreach (var game in sortedGames)
        {
            if (string.IsNullOrWhiteSpace(game.Name))
                continue;

            var reShade = await _modDetectionService.DetectInstalledReShadeAsync(game.ExecutablePath!);
            var renoDx = await _modDetectionService.DetectInstalledRenoDXAsync(game.ExecutablePath!);

            // TODO(): handle multiple mods found with user choice
            game.ReShade = reShade.Value?.FirstOrDefault();
            game.RenoDX = renoDx.Value?.FirstOrDefault();

            var renoDxUpdateResult = game.RenoDX is not null
                ? _updateCheckService.CheckRenoDXUpdate(game.RenoDX)
                : null;

            var compatibleMod = FindCompatibleMod(GameNameHelper.StripCollectionPartSuffix(game.Name),
                _modCatalog.GetRenoDXWikiMods());
            var compatibleGenericMod = compatibleMod is null
                ? FindGenericMod(GameNameHelper.StripCollectionPartSuffix(game.Name),
                    _modCatalog.GetRenoDXGenericWikiMods())
                : null;

            artworkTasks.Add(_gameArtworkService.EnrichGameArtworkAsync(game));

            var gameEntry = _gameEntryAssembler.Assemble(game);

            results.Add(new GameInitResultDto(
                game,
                compatibleMod,
                compatibleGenericMod,
                gameEntry.ReShadeUpdate,
                renoDxUpdateResult,
                gameEntry
            ));
        }

        await Task.WhenAll(artworkTasks);

        return new RefreshLibraryResponse(results, success, warnings, errorMessage);
    }

    private static RenoDXModInfoDto? FindCompatibleMod(string? gameName, ImmutableArray<RenoDXModInfoDto> mods)
    {
        if (string.IsNullOrWhiteSpace(gameName))
            return null;

        var candidates = mods.Where(m =>
            GameNameHelper.IsLikelySameGame(gameName, m.Name)).ToList();

        var normalizedGameName = GameNameHelper.NormalizeName(gameName);

        return candidates.FirstOrDefault(m =>
                   GameNameHelper.NormalizeName(m.Name) == normalizedGameName) ??
               candidates.FirstOrDefault();
    }

    private static RenoDXGenericModInfoDto? FindGenericMod(string? gameName,
        ImmutableArray<RenoDXGenericModInfoDto> mods)
    {
        if (string.IsNullOrWhiteSpace(gameName))
            return null;

        var candidates = mods.Where(m =>
            GameNameHelper.IsLikelySameGame(gameName, m.Name)).ToList();

        var normalizedGameName = GameNameHelper.NormalizeName(gameName);

        return candidates.FirstOrDefault(m =>
                   GameNameHelper.NormalizeName(m.Name) == normalizedGameName) ??
               candidates.FirstOrDefault();
    }
}
