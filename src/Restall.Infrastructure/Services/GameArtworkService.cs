using System.Text.Json;
using Restall.Application.Interfaces.Driven;

namespace Restall.Infrastructure.Services;

public class GameArtworkService : IGameArtworkService
{
    private readonly ILogService _logService;
    private readonly HttpClient  _httpClient;
    private readonly IPathService _pathService;

    private static readonly JsonSerializerOptions s_options = new();
    
    public GameArtworkService(ILogService logService, 
        HttpClient httpClient,
        IPathService pathService)
    {
        _logService = logService;
        _httpClient = httpClient;
        _pathService = pathService;
    }
    
    
    
    
    
    public Task EnrichGameArtworkAsync(string slug)
    {
        throw new NotImplementedException();
    }
}