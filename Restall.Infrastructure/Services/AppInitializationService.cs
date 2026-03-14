using Restall.Application.DTOs;
using Restall.Application.Helpers;
using Restall.Application.Interfaces;

namespace Restall.Infrastructure.Services;

public class AppInitializationService : IAppInitializationService
{
    private readonly IGameDetectionService _gameDetectionService;
    private readonly IModDetectionService _modDetectionService;
    private readonly IParseService _parseService;
    private readonly ISteamGridDbService _steamGridDbService;
    private readonly IUpdateCheckService _updateCheckService;

    public AppInitializationService(
        IGameDetectionService gameDetectionService,
        IModDetectionService modDetectionService,
        IParseService parseService,
        ISteamGridDbService steamGridDbService,
        IUpdateCheckService updateCheckService
        )
    {
        _gameDetectionService = gameDetectionService;
        _modDetectionService = modDetectionService;
        _parseService = parseService;
        _steamGridDbService = steamGridDbService;
        _updateCheckService = updateCheckService;
    }

    public async Task<AppInitializationResultDto> InitializeAsync(IProgress<GameScanProgressReportDto>? progress = null)
    {
        var gamesTask = _gameDetectionService.FindGames(progress);
        var parseTask = _parseService.FetchAvailableModsAsync();

        await Task.WhenAll(gamesTask, parseTask);
        
        var gameScanResults = gamesTask.Result;
        var wikiResults = parseTask.Result;
        var results = new List<GameInitResultDto>();

        var sortedGames = gameScanResults.Games
            .OrderBy(g => g.Name);

        foreach (var game in sortedGames)
        {
            var reShade = await _modDetectionService.DetectInstalledReShadeAsync(game!.ExecutablePath!);
            var renoDx = await _modDetectionService.DetectInstalledRenoDXAsync(game!.ExecutablePath!);
            
            game.ReShade = reShade?.FirstOrDefault();
            game.RenoDX = renoDx?.FirstOrDefault();
            
            await _steamGridDbService.EnrichGameArtworkAsync(game);

            var reShadeUpdateResult = game.ReShade is not null
                ? _updateCheckService.CheckReShadeUpdate(game.ReShade)
                : null;

            var renoDxUpdateResult = game.RenoDX is not null
                ? _updateCheckService.CheckRenoDXUpdate(game.RenoDX)
                : null;

            var compatibleMod = FindCompatibleMod(game.Name, wikiResults.WikiMods);
            var compatibleGenericMod = compatibleMod is null
                ? FindGenericMod(game.Name, wikiResults.GenericWikiMods)
                : null;

            results.Add(new GameInitResultDto(game, 
                compatibleMod, 
                compatibleGenericMod,
                reShadeUpdateResult,
                renoDxUpdateResult
                ));
        }

        return new AppInitializationResultDto(results, true);
    }

    private static RenoDXModInfoDto? FindCompatibleMod(string? gameName, IReadOnlyList<RenoDXModInfoDto> mods)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return null;

        var key = GameNameHelper.NormalizeName(gameName);

        return mods.FirstOrDefault(m => m.Status != "💀" && GameNameHelper.NormalizeName(m.Name) == key)
            ?? mods.FirstOrDefault(m => m.Status != "💀" && GameNameHelper.FuzzyNameMatch(key, GameNameHelper.NormalizeName(m.Name)));
    }

    private static RenoDXGenericModInfoDto? FindGenericMod(string? gameName, IReadOnlyList<RenoDXGenericModInfoDto> mods)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return null;

        var key = GameNameHelper.NormalizeName(gameName);

        return mods.FirstOrDefault(m => m.Status != "💀" && GameNameHelper.NormalizeName(m.Name) == key)
            ?? mods.FirstOrDefault(m => m.Status != "💀" && GameNameHelper.FuzzyNameMatch(key, GameNameHelper.NormalizeName(m.Name)));
    }
}