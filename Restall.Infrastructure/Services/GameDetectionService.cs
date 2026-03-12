using System.Collections.Concurrent;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;


public class GameDetectionService : IGameDetectionService
{
    private readonly ILogService _logService;
    private readonly IEnumerable<IPlatformScannerService> _platformScannerService;
    private readonly IEngineDetectionService _engineDetectionService;
    
    private static readonly ParallelOptions s_engineParallelOptions = new()
    {
        MaxDegreeOfParallelism =  2
    };
    
    public GameDetectionService(
        ILogService logService,
        IEnumerable<IPlatformScannerService> platformScannerService,
        IEngineDetectionService engineDetectionService)
    {
        _logService = logService;
        _engineDetectionService = engineDetectionService;
        _platformScannerService = platformScannerService;
    }
    
     public async Task<List<Game?>> FindGames()
     {
          try
          {
              
              var scanTasks = _platformScannerService.Select(s => s.ScanAsync());
              var results = await Task.WhenAll(scanTasks);
              var allGames = results.SelectMany(g => g).ToList();
         
              var deduped = allGames.GroupBy(g => g.Name)
                  .Select(g => g.First())
                  .GroupBy(g => g.InstallFolder,StringComparer.OrdinalIgnoreCase)
                  .Select(g => g.First())
                  .ToList<Game?>();

              
              var engineCache = new ConcurrentDictionary<string, (string? path, Game.Engine engine)>(
                  StringComparer.OrdinalIgnoreCase);

              await Task.Run(() =>
              {
                  Parallel.ForEach(deduped, s_engineParallelOptions, game =>
                  {
                      if (string.IsNullOrEmpty(game.InstallFolder)) return;

                      var rootKey = (Helper.NormalizePath(game.InstallFolder) ?? game.InstallFolder).ToLowerInvariant();

                      if (engineCache.TryGetValue(rootKey, out var cached))
                      {
                          game.ExecutablePath = cached.path;
                          game.EngineName     = cached.engine;
                          return;
                      }

                      var (executablePath, engine) =
                          _engineDetectionService.DetectExecutablePathAndEngine(game.InstallFolder);
                      game.ExecutablePath = executablePath;
                      game.EngineName     = engine;
                      engineCache[rootKey] = (executablePath, engine);
                  });
              });
                  
              
             

              

              return deduped
                  .Where(g => !string.IsNullOrEmpty(g.InstallFolder))
                  .Cast<Game>()
                  .ToList();
         
          }
          catch (Exception ex)
          {
              await _logService.LogErrorAsync($"Something went wrong with FindGames: {ex.Message}");
              return [];
          }
         
         
         
         
     }

    
}