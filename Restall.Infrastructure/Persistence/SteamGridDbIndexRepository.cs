using Restall.Application.Interfaces;
using System.Text.Json;

namespace Restall.Infrastructure.Persistence;

internal sealed class SteamGridDbIndexRepository : ISteamGridDbIndexRepository
{
    private readonly ILogService _logService;
    private readonly ICachePathService _cachePathService;
    private const string s_indexFileName = "index.json";
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };
    private readonly Dictionary<string, int>? _index = [];
    private readonly string _indexFilePath;

    public SteamGridDbIndexRepository(ILogService logService,
        ICachePathService cachePathService)
    {
        _logService = logService;
        _cachePathService = cachePathService;
        
        _indexFilePath = Path.Combine(cachePathService.GetSgdbCacheDirectory(), s_indexFileName);

        Directory.CreateDirectory(cachePathService.GetSgdbCacheDirectory());
        _index = LoadIndex();
        
    }
    
    public int? TryGetSteamGridDbId(string cacheKey) => _index.TryGetValue(cacheKey, out var steamGridDbId) ? steamGridDbId : null;
    

    public async Task SaveSteamGridDbIdAsync(string cacheKey, int steamGridDbId)
    {
        _index![cacheKey] = steamGridDbId;
        ;
        try
        {
            var json = JsonSerializer.Serialize(_index, s_jsonOptions);
            await File.WriteAllTextAsync(_indexFilePath, json);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to save index file {_indexFilePath}", ex);
        }
    }
    
    private Dictionary<string, int>? LoadIndex()
    {

        try
        {
            if (!File.Exists(_indexFilePath)) return [];
            var json = File.ReadAllText(_indexFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? [];
        }
        catch(Exception ex)
        {
            _logService.LogError($"Failed to load index file {_indexFilePath}", ex);
            return [];
        }
    }
    
}