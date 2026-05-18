using System.Collections.Immutable;
using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public sealed class RefreshLibraryUseCase : IRefreshLibraryUseCase, ILightRefreshLibraryUseCase
{
    private readonly ILogService _logService;
    private readonly IGameDetectionService _gameDetectionService;
    private readonly IGameArtworkService _gameArtworkService;
    private readonly IModDetectionService _modDetectionService;
    private readonly IUpdateCheckService _updateCheckService;
    private readonly IVersionCatalog _versionCatalog;
    private readonly IModCatalog _modCatalog;

    public RefreshLibraryUseCase(
        ILogService logService,
        IGameDetectionService gameDetectionService,
        IGameArtworkService gameArtworkService,
        IModDetectionService modDetectionService,
        IUpdateCheckService updateCheckService,
        IVersionCatalog versionCatalog,
        IModCatalog modCatalog
    )
    {
        _logService = logService;
        _gameDetectionService = gameDetectionService;
        _gameArtworkService = gameArtworkService;
        _modDetectionService = modDetectionService;
        _updateCheckService = updateCheckService;
        _versionCatalog = versionCatalog;
        _modCatalog = modCatalog;
    }

    public async Task<RefreshLibraryResultDto> ExecuteFullRescanAsync(IProgress<GameScanProgressReportDto>? progress = null)
    {
        var gameTask = _gameDetectionService.FindGamesAsync(progress);
        var versionTask = _versionCatalog.FetchVersionsAsync();
        var wikiTask = _modCatalog.FetchModsAsync();

        await Task.WhenAll(gameTask, versionTask, wikiTask);

        var gameScanResults = gameTask.Result;

        var games = gameScanResults.Games.OrderBy(g => g.Name);
        return await BuildResultAsync(games, gameScanResults.IsSuccess, gameScanResults.Message);
    }

    public async Task<RefreshLibraryResultDto> ExecuteLightRescanAsync(IReadOnlyList<Game> existingGames, IProgress<GameScanProgressReportDto>? progress = null)
    {
        await Task.WhenAll(_versionCatalog.FetchVersionsAsync(), _modCatalog.FetchModsAsync());

        return await BuildResultAsync(existingGames.OrderBy(g => g.Name), true, null);
    }

    private async Task<RefreshLibraryResultDto> BuildResultAsync(IOrderedEnumerable<Game> sortedGames, bool success, string? errorMessage)
    {
        HashSet<Task> artworkTasks = [];
        List<GameInitResultDto> results = [];

        foreach (var game in sortedGames)
        {
            var reShade = await _modDetectionService.DetectInstalledReShadeAsync(game.ExecutablePath!);
            var renoDx = await _modDetectionService.DetectInstalledRenoDXAsync(game.ExecutablePath!);

            game.ReShade = reShade.Value?.FirstOrDefault();
            game.RenoDX = renoDx.Value?.FirstOrDefault();

            var reShadeUpdateResult = game.ReShade is not null
                ? _updateCheckService.CheckReShadeUpdate(game.ReShade)
                : null;

            var renoDxUpdateResult = game.RenoDX is not null
                ? _updateCheckService.CheckRenoDXUpdate(game.RenoDX)
                : null;

            var compatibleMod = FindCompatibleMod(game.Name, _modCatalog.GetRenoDXWikiMods());
            var compatibleGenericMod = compatibleMod is null
                ? FindGenericMod(game.Name, _modCatalog.GetRenoDXGenericWikiMods())
                : null;

            artworkTasks.Add(_gameArtworkService.EnrichGameArtworkAsync(game));

            results.Add(new GameInitResultDto(
                game,
                compatibleMod,
                compatibleGenericMod,
                reShadeUpdateResult,
                renoDxUpdateResult
            ));
        }

        await Task.WhenAll(artworkTasks);

        return new RefreshLibraryResultDto(results, success, errorMessage);
    }

    private static RenoDXModInfoDto? FindCompatibleMod(string? gameName, ImmutableArray<RenoDXModInfoDto> mods)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return null;

        var key = GameNameHelper.NormalizeName(gameName);

        return mods.FirstOrDefault(m => m.Status != "💀" && GameNameHelper.NormalizeName(m.Name) == key)
               ?? mods.FirstOrDefault(m =>
                   m.Status != "💀" && GameNameHelper.FuzzyNameMatch(key, GameNameHelper.NormalizeName(m.Name)));
    }

    private static RenoDXGenericModInfoDto? FindGenericMod(string? gameName,
        ImmutableArray<RenoDXGenericModInfoDto> mods)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return null;

        var key = GameNameHelper.NormalizeName(gameName);

        return mods.FirstOrDefault(m => m.Status != "💀" && GameNameHelper.NormalizeName(m.Name) == key)
               ?? mods.FirstOrDefault(m =>
                   m.Status != "💀" && GameNameHelper.FuzzyNameMatch(key, GameNameHelper.NormalizeName(m.Name)));
    }
}