using Restall.Application.DTOs;
using Restall.Application.Interfaces;

namespace Restall.Infrastructure.Services;

public class AppInitializationService : IAppInitializationService
{
    private readonly IGameDetectionService _gameDetectionService;
    private readonly IModDetectionService _modDetectionService;
    private readonly IParseService _parseService;

    public AppInitializationService(
        IGameDetectionService gameDetectionService,
        IModDetectionService modDetectionService,
        IParseService parseService
        )
    {
        _gameDetectionService = gameDetectionService;
        _modDetectionService = modDetectionService;
        _parseService = parseService;
    }

    public async Task<AppInitializationResultDto> InitializeAsync(IProgress<GameScanProgressReportDto>? progress = null)
    {
        var gamesTask = _gameDetectionService.FindGames();
        var parseTask = _parseService.FetchAvailableModVersionsAsync();

        await Task.WhenAll(gamesTask, parseTask);

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

            var compatibleMod = _parseService.GetCompatibleRenoDXMod(game.Name);
            var compatibleGenericMod = compatibleMod is null
                ? _parseService.GetGenericRenoDXInfo(game.Name)
                : null;

            results.Add(new GameInitResultDto(game, compatibleMod, compatibleGenericMod));
        }

        return new AppInitializationResultDto(results);
    }
}