using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;


public class GameDetectionService : IGameDetectionService
{
    private readonly ILogService _logService;
    private readonly IEnumerable<IPlatformScannerService> _platformScannerService;

    public GameDetectionService(
        ILogService logService,
        IEnumerable<IPlatformScannerService> platformScannerService)
    {
        _logService = logService;
        _platformScannerService = platformScannerService;
    }

    public async Task<List<Game?>> FindGames()
    {
        try
        {
            var scanTasks = _platformScannerService.Select(s =>
            {
                _logService.LogInfoAsync($"Starting {s.Platform} scan....");
                return s.ScanAsync();
            });
            var results = await Task.WhenAll(scanTasks);
            var allGames = results.SelectMany(g => g).ToList();

            var sortGames = allGames.GroupBy(g => g.Name)
                .Select(g => g.First())
                .GroupBy(g => g.InstallFolder)
                .Select(g => g.First())
                .ToList<Game?>();

            return sortGames;

        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Something went wrong with FindGames: {ex.Message}");
            return new List<Game?>();
        }
    }
    
}