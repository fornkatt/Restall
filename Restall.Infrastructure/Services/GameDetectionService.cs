using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Collections.Concurrent;

namespace Restall.Infrastructure.Services;

/// <summary>
/// For more information how I handle each scanner, check out GOGScanner
///
/// Collecting all the results across all the scanners and remove duplicates that is being found.
/// The main purpose of GameDetectionService is I am running the detection in parallel by using ConcurrentDictionary 
/// and it is simply because I want to avoid scanning the same folder twice across the parallel threads 
/// only games with a detected "ExecutablePath" are returned as valid.
/// </summary>
internal sealed class GameDetectionService : IGameDetectionService
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
    
     public async Task<GameScanResultDto> FindGamesAsync(IProgress<GameScanProgressReportDto>? progress = null)
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
                  
                  if(result.Message is not null)
                      allErrors.Add(result.Message);

                  progress?.Report(new GameScanProgressReportDto(
                      CompletedPlatform: result.Platform.ToString(),
                      ScannersCompleted: completed,
                      TotalScanners:     totalScanners,
                      IsSuccess:           result.IsSuccess,
                      Message:      result.Message
                      ));
                  
              }
              
              //Remove duplicate entries by checking installfolder and sorting by platformId
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
                      if (game is null || string.IsNullOrWhiteSpace(game.InstallFolder)) return;

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
              
              //Validating after the smart sorting
              var validGames = deduped
                  .Where(g => g is not null && !string.IsNullOrWhiteSpace(g.ExecutablePath))
                  .ToList();

              
              return new GameScanResultDto(
                  Platform:     Game.Platform.Unknown,
                  Games:        validGames!,
                  IsSuccess:      validGames.Count > 0,
                  Message: allErrors.Count > 0 ? string.Join("; ", allErrors) : null);
              

          }
          catch (Exception ex)
          {
              await _logService.LogErrorAsync($"Failed to scan libraries",ex);
              return new GameScanResultDto(
                  Platform:     Game.Platform.Unknown,
                  Games:        [],
                  IsSuccess:      false,
                  Message: "Failed to scan game libraries. Please try rescanning.");
          }
         
         
         
         
     }

    
}