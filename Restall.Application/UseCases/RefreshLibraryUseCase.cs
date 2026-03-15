using Restall.Application.DTOs;
using Restall.Application.Interfaces;

namespace Restall.Application.UseCases;

public class RefreshLibraryUseCase : IRefreshLibraryUseCase
{
    private readonly ILogService _logService;
    private readonly IGameDetectionService _gameDetectionService;
    private readonly ISteamGridDbService _steamGridDbService;
    private readonly IModDetectionService _modDetectionService;

    public RefreshLibraryUseCase(
        ILogService logService,
        IGameDetectionService gameDetectionService,
        ISteamGridDbService steamGridDbService,
        IModDetectionService modDetectionService)
    {
        _logService = logService;
        _gameDetectionService = gameDetectionService;
        _steamGridDbService = steamGridDbService;
        _modDetectionService = modDetectionService;
        
    }
    
    public async Task<AppInitializationResultDto> ExecuteAsync(IProgress<GameScanProgressReportDto>? progress = null)
    {
        var gameScanResults = await _gameDetectionService.FindGames(progress);
        var results = new List<GameInitResultDto>();
        
        var sortedGames = gameScanResults.Games
            .OrderBy(g => g.Name);

        foreach (var game in sortedGames)
        {
            await _steamGridDbService.EnrichGameArtworkAsync(game);
            
        }
        
        
        return new AppInitializationResultDto(results, gameScanResults.Success, gameScanResults.ErrorMessage);
        
    }
}