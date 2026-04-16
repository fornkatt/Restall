using System.Text.Json;
using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

public class GameArtworkService : IGameArtworkService
{
    private readonly ILogService _logService;
    private readonly HttpClient _httpClient;
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

    public async Task EnrichGameArtworkAsync(Game game)
    {
        try
        {
            var slug = GameNameHelper.NormalizeName(game.Name ?? string.Empty);
            var coverPath = _pathService.GetGameArtworkCover(slug);
            var iconPath = _pathService.GetGameArtThumbnailPath(slug);
            
            Directory.CreateDirectory(Path.GetDirectoryName(coverPath)!);
            
            await DownloadCoverUrlIfMissingAsync(game.Name ?? string.Empty, coverPath);
            await ExtractIconIfMissingAsync(game.ExecutablePath, game.Name,iconPath);
            game.GameCoverPathString = File.Exists(coverPath) ? coverPath : string.Empty;
        }
        catch(Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to enrich game artwork for [{game.Name}]", ex);
        }
    }

    private async Task ExtractIconIfMissingAsync(string? executablePath, string? gameName, string? iconPath)
    {
        if (File.Exists(iconPath)) return;
        
        var exePath = ResolveMainExecutablePath(executablePath, gameName);
        if (exePath is null) return;
        try
        {
            //TODO: PEICONHELPER METHOD
            
            
        }
        catch(Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to get the iconPath", ex);
        }

    }
    
    
    private static string? ResolveMainExecutablePath(string? executablePath, string? gameName)
    {
        if (string.IsNullOrWhiteSpace(executablePath) || !Directory.Exists(executablePath))
            return null;
        //Priority on getting the game application.exe over i.e. Ubisoft Connect Installer
        var exes = Directory.GetFiles(executablePath, "*.exe")
            .Where(e =>
                !GameScanHelper.NonGameExecutable(Path.GetFileNameWithoutExtension(e)))
            .ToArray();

        switch (exes.Length)
        {
            case 0:
                return null;
            case 1:
                return exes[0];
                
        }
        
        var normalized = GameNameHelper.NormalizeName(gameName ?? string.Empty);
        var stripped = GameNameHelper.StripEditionSuffix(normalized);

        //Find the first best match 
        var match = exes.FirstOrDefault(e =>
        {
           var exeName = GameNameHelper.NormalizeName(Path.GetFileNameWithoutExtension(e));
           
           return exeName.Contains(normalized) ||
                  exeName.Contains(stripped) ||
                  exeName.Contains(exeName) ||
                  GameNameHelper.FuzzyNameMatch(normalized, exeName) ||
                  GameNameHelper.FuzzyNameMatch(stripped, exeName);
        });
        
        return match ?? exes.OrderByDescending(e => new FileInfo(e).Length).First();
    }

    private async Task DownloadCoverUrlIfMissingAsync(string gameName, string coverPath)
    {
        if (File.Exists(coverPath)) return;

        var url = await TryResolveGameCoverUrlAsync(gameName);
        if (url is null)
        {
            await _logService.LogWarningAsync($"Couldn't find game cover url for [{gameName}]");
            return;
        }

        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(coverPath, bytes);
            await _logService.LogInfoAsync($"Downloaded cover for $[{gameName}]");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to download cover for [{gameName}]", ex);
        }
    }

    private async Task<string?> ResolveGameCoverUrlAsync(string gameName)
    {
        //Two-pass: checking raw game name and stipped edition
        var url = await TryResolveGameCoverUrlAsync(gameName);
        if (url is not null) return url;
        var stripped = GameNameHelper.StripEditionSuffix(gameName);
        if (!string.Equals(stripped, gameName, StringComparison.OrdinalIgnoreCase))
            url = await TryResolveGameCoverUrlAsync(stripped);
        return url;
    }

    private async Task<string?> TryResolveGameCoverUrlAsync(string gameName)
    {
        var encodedName = Uri.EscapeDataString(gameName);
        
        var candidates = new[]
        {
            $"File:{encodedName}_cover.jpg",
            $"File:{encodedName}_cover.png",
            $"File:{encodedName}_Cover.jpg",
            $"File:{encodedName}_Cover.png"
        };

        foreach (var fileTitle in candidates)
        {
            var apiUrl =
                $"https://www.pcgamingwiki.com/w/api.php?action=query&titles={fileTitle}&prop=imageinfo&iiprop=url&format=json";

            try
            {
                using var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) continue;

                var json = await response.Content.ReadAsStringAsync();
                var url = ParseImageUrl(json);
                if (url is not null) return url;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"PC Gaming Wiki API failed to parse request for [{fileTitle}]", ex);
            }
        }

        return null;
    }

    private string? ParseImageUrl(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement
            .GetProperty("query")
            .GetProperty("pages");
        foreach (var page in root.EnumerateObject())
        {
            if (page.Name.StartsWith("-")) continue;
            if (page.Value.TryGetProperty("imageinfo", out var imageInfo)
                && imageInfo.GetArrayLength() > 0)
            {
                var urlProp = imageInfo[0].GetProperty("url");
                return urlProp.GetString();
            }
        }

        return null;
    }
}