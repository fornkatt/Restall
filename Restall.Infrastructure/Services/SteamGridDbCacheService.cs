using System.Text.Json;
using Restall.Application.Interfaces;

namespace Restall.Infrastructure.Services;

public class SteamGridDbCacheService : ISteamGridDbCacheService
{
    private readonly ILogService _logService;

    private const string s_cacheFolderName = "SGDB";
    private const string s_indexFileName = "index.json";
    private const string s_bannerFileName = "banner.png";
    private const string s_iconFileName = "icon.png";
    private const string s_logoFileName = "logo.png";

    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };
    private readonly string _sgdbCacheBaseDir;
    private readonly string _indexFilePath;

    private readonly Dictionary<string, int>? _index;

    public SteamGridDbCacheService(ILogService logService)
    {
        _logService       = logService;
        _sgdbCacheBaseDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Cache", s_cacheFolderName);
        _indexFilePath    = Path.Combine(_sgdbCacheBaseDir, s_indexFileName);

        Directory.CreateDirectory(_sgdbCacheBaseDir);
        _index = LoadIndex();
    }

    public int? TryGetSteamGridDbId(string cacheKey) =>
        _index.TryGetValue(cacheKey, out var steamGridDbId) ? steamGridDbId : null;

    
    public string GetBannerPath(int steamGridDbId) =>
        Path.Combine(_sgdbCacheBaseDir, steamGridDbId.ToString(), s_bannerFileName);

    public string GetThumbnailPath(int steamGridDbId) =>
        Path.Combine(_sgdbCacheBaseDir, steamGridDbId.ToString(), s_iconFileName);

    public string GetLogoPath(int steamGridDbId) => Path.Combine(_sgdbCacheBaseDir, steamGridDbId.ToString(), s_logoFileName);

    public bool BannerExists(int steamGridDbId) => File.Exists(GetBannerPath(steamGridDbId));

    public bool ThumbnailExists(int steamGridDbId) => File.Exists(GetThumbnailPath(steamGridDbId));
    public bool LogoExists(int steamGridDbId) => File.Exists(GetLogoPath(steamGridDbId));

    public async Task SaveSteamGridDbIdAsync(string cacheKey, int steamGridDbId)
    {
        _index![cacheKey] = steamGridDbId;
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
        catch
        {
            _logService.LogError($"Failed to load index file {_indexFilePath}");
            return [];
        }
    }
}