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

    public AppInitializationService(
        IGameDetectionService gameDetectionService,
        IModDetectionService modDetectionService,
        IParseService parseService,
        ISteamGridDbService steamGridDbService
        )
    {
        _gameDetectionService = gameDetectionService;
        _modDetectionService = modDetectionService;
        _parseService = parseService;
        _steamGridDbService = steamGridDbService;
    }

    public async Task<AppInitializationResultDto> InitializeAsync(IProgress<GameScanProgressReportDto>? progress = null)
    {
        var gamesTask = _gameDetectionService.FindGames();
        var parseTask = _parseService.FetchAvailableModsAsync();

        await Task.WhenAll(gamesTask, parseTask);

        var wikiResults = parseTask.Result;
        var results = new List<GameInitResultDto>();

        var sortedGames = gamesTask.Result
            .Where(g => g is not null)
            .OrderBy(g => g!.Name);

        foreach (var game in sortedGames)
        {
            var reShade = await _modDetectionService.DetectInstalledReShadeAsync(game!.ExecutablePath!);
            var renoDx = await _modDetectionService.DetectInstalledRenoDXAsync(game!.ExecutablePath!);
            
            
            game.ReShade = reShade?.FirstOrDefault();
            game.RenoDX = renoDx?.FirstOrDefault();
            
            await _steamGridDbService.EnrichGameArtworkAsync(game);


            var compatibleMod = FindCompatibleMod(game.Name, wikiResults.WikiMods);
            var compatibleGenericMod = compatibleMod is null
                ? FindGenericMod(game.Name, wikiResults.GenericWikiMods)
                : null;

            results.Add(new GameInitResultDto(game, compatibleMod, compatibleGenericMod));
        }

        return new AppInitializationResultDto(results);
    }

    private static RenoDXModInfoDto? FindCompatibleMod(string? gameName, IReadOnlyList<RenoDXModInfoDto> mods)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return null;

        var key = GameNameHelper.NormalizeName(gameName);

        return mods.FirstOrDefault(m => m.Status != "💀" && GameNameHelper.NormalizeName(m.Name) == key)
            ?? mods.FirstOrDefault(m => m.Status != "💀" && FuzzyNameMatch(key, GameNameHelper.NormalizeName(m.Name)));
    }

    private static RenoDXGenericModInfoDto? FindGenericMod(string? gameName, IReadOnlyList<RenoDXGenericModInfoDto> mods)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return null;

        var key = GameNameHelper.NormalizeName(gameName);

        return mods.FirstOrDefault(m => m.Status != "💀" && GameNameHelper.NormalizeName(m.Name) == key)
            ?? mods.FirstOrDefault(m => m.Status != "💀" && FuzzyNameMatch(key, GameNameHelper.NormalizeName(m.Name)));
    }

    private static bool FuzzyNameMatch(string a, string b)
    {
        if (!a.Contains(b) && !b.Contains(a))
            return false;

        var aWords = a.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var bWords = b.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var bSet = new HashSet<string>(bWords);

        int shared = aWords.Count(w => bSet.Contains(w));
        int maxWords = Math.Max(aWords.Length, bWords.Length);

        return maxWords > 0 && (double)shared / maxWords >= 0.5;
    }
}