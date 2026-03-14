using System.Collections.Concurrent;
using Restall.Application.DTOs;
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
    
     public async Task<GameScanResultDto> FindGames(IProgress<GameScanProgressReportDto>? progress = null)
     {
          try
          {

              var scanners = _platformScannerService.ToList();
              var totalScanners = scanners.Count;
              var completed = 0;
              var allGames = new List<Game>();
              var allErrors = new List<string>();

              foreach (var scanner in scanners)
              {
                  
                  var result = await scanner.ScanAsync();
                  completed++;
                  allGames.AddRange(result.Games);
                  
                  if(result.ErrorMessage is not null)
                      allErrors.Add(result.ErrorMessage);

                  progress?.Report(new GameScanProgressReportDto(
                      CompletedPlatform: result.Platform.ToString(),
                      ScannersCompleted: completed,
                      TotalScanners:     totalScanners,
                      Success:           result.Success,
                      ErrorMessage:      result.ErrorMessage
                      ));
                  
              }
              
              var deduped = allGames
                  .GroupBy(g => g.InstallFolder, StringComparer.OrdinalIgnoreCase)
                  .Select(g => g.OrderByDescending(x => x.PlatformId != null).First())
                  .ToList<Game?>();
              
              var engineCache = new ConcurrentDictionary<string, (string? path, Game.Engine engine)>(
                  StringComparer.OrdinalIgnoreCase);

              await Task.Run(() =>
              {
                  Parallel.ForEach(deduped, s_engineParallelOptions, game =>
                  {
                      if (string.IsNullOrEmpty(game.InstallFolder)) return;

                      var rootKey = (GameScanHelper.NormalizePath(game.InstallFolder) ?? game.InstallFolder).ToLowerInvariant();

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
              
              var validGames = deduped
                  .Where(g => !string.IsNullOrEmpty(g.ExecutablePath))
                  .ToList();

              
              return new GameScanResultDto(
                  Platform:     Game.Platform.Unknown,
                  Games:        validGames!,
                  Success:      validGames.Count > 0,
                  ErrorMessage: allErrors.Count > 0 ? string.Join("; ", allErrors) : null);
              

          }
          catch (Exception ex)
          {
              await _logService.LogErrorAsync($"Something went wrong with FindGames: {ex.Message}");
              return new GameScanResultDto(
                  Platform:     Game.Platform.Unknown,
                  Games:        [],
                  Success:      false,
                  ErrorMessage: ex.Message);
          }
         
         
         
         
     }

    
}