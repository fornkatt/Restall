using System.Text.Json;
using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;


namespace Restall.Infrastructure.Services;

public class GameArtworkService : IGameArtworkService
{
    private readonly ILogService  _logService;
    private readonly HttpClient   _httpClient;
    private readonly IPathService _pathService;
    private readonly IImageResizeService _imageResizeService;

    // PCGamingWiki Cargo API — fallback for all platforms
    private const string PcgwCargoByAppIdUrl   = "https://www.pcgamingwiki.com/w/api.php?action=cargoquery&tables=Infobox_game&fields=Infobox_game.Cover_URL&where=Infobox_game.Steam_AppID%20HOLDS%20%22{0}%22&format=json";
    private const string PcgwCargoByPageNameUrl = "https://www.pcgamingwiki.com/w/api.php?action=cargoquery&tables=Infobox_game&fields=Infobox_game.Cover_URL&where=Infobox_game._pageName%3D%22{0}%22&format=json";
    private const string PcgwSearchUrl         = "https://www.pcgamingwiki.com/w/api.php?action=query&list=search&srsearch={0}&srnamespace=0&srlimit=3&format=json";
    private const string PcgwCargoByPageIdUrl  = "https://www.pcgamingwiki.com/w/api.php?action=cargoquery&tables=Infobox_game&fields=Infobox_game.Cover_URL&where=Infobox_game._pageID%3D{0}&format=json";

    // GOG public API
    private const string GogApiV2ProductUrl = "https://api.gog.com/v2/games/{0}";

    

    public GameArtworkService(ILogService logService, 
        HttpClient httpClient, 
        IPathService pathService,
        IImageResizeService imageResizeService)
    {
        _logService  = logService;
        _httpClient  = httpClient;
        _pathService = pathService;
        _imageResizeService = imageResizeService;

        Directory.CreateDirectory(pathService.GetArtworkCacheDirectory());
    }

    public async Task EnrichGameArtworkAsync(Game game)
    {
        try
        {
            var slug      = GameNameHelper.NormalizeName(game.Name ?? string.Empty);
            var coverPath = _pathService.GetGameArtworkCover(slug);
            var iconPath  = _pathService.GetGameArtThumbnailPath(slug);

            Directory.CreateDirectory(Path.GetDirectoryName(coverPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(iconPath)!);

            await DownloadCoverIfMissingAsync(game, coverPath);
            await ExtractIconIfMissingAsync(game.ExecutablePath, game.Name, iconPath);

            game.GameCoverPathString = File.Exists(coverPath) ? coverPath : string.Empty;
            game.ThumbnailPathString = File.Exists(iconPath)  ? iconPath  : string.Empty;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to enrich game artwork for [{game.Name}]", ex);
        }
    }

    private async Task DownloadCoverIfMissingAsync(Game game, string coverPath)
    {
        if (File.Exists(coverPath)) return;

        var source = await ResolveCoverSourceAsync(game);
        if (source is not null)
            await CopyCoverAsync(game.Name, coverPath, source);

        if (!File.Exists(coverPath))
            await _logService.LogWarningAsync($"Couldn't find cover for [{game.Name}]");
    }

    // source is either a local file path or a remote URL
    private async Task CopyCoverAsync(string? gameName, string coverPath, string source)
    {
        try
        {
            if (source.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                await _logService.LogInfoAsync($"Cover [{gameName}] → downloading from [{source}]");
                await TryDownloadCoverAsync(gameName, coverPath, source);
            }
            else if (File.Exists(source))
            {
                await _logService.LogInfoAsync($"Cover [{gameName}] → copying from local [{source}]");
                File.Copy(source, coverPath, overwrite: true);
            }
            
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to copy cover for [{gameName}]", ex);
        }
    }

    // Platform dispatch -------------------------------------------------------------------

    private async Task<string?> ResolveCoverSourceAsync(Game game) =>
        game.PlatformName switch
        {
            Game.Platform.Steam   => await TryGetSteamLocalCover(game)
                                     ?? await ResolvePcgwCoverUrlAsync(game),

            Game.Platform.GOG     => await TryGetGogLocalCover(game)
                                     ?? await TryResolveGogApiCoverAsync(game)
                                     ?? await TryResolveHeroicCoverAsync(game)
                                     ?? await ResolvePcgwCoverUrlAsync(game),

            Game.Platform.Epic    => await TryResolveHeroicCoverAsync(game)
                                     ?? await ResolvePcgwCoverUrlAsync(game),

            //Fallback
            _                     => await ResolvePcgwCoverUrlAsync(game)
        };

    // Steam -------------------------------------------------------------------------------

    private async Task<string?> TryGetSteamLocalCover(Game game)
    {
        if (string.IsNullOrWhiteSpace(game.PlatformId)) return null;

        var appId     = StripPrefix(game.PlatformId, "steam:");
        var steamRoot = FindSteamRoot();
        if (steamRoot is null)
        {
             await _logService.LogWarningAsync($"Couldn't find steam root for [{game.Name}] Redirecting to PC Gaming Wiki");
            return null;
        }
        
        var libCacheDir = Path.Combine(steamRoot, "appcache", "librarycache");
        var directPath = Path.Combine(libCacheDir, appId, "library_600x900.jpg");
        if (File.Exists(directPath)) return directPath;
       
        //Fallback for folders
        var appDir = Path.Combine(libCacheDir, appId);
        if(!Directory.Exists(appDir)) return null;
        string[] preferredNames = ["library_600x900.jpg", "library_capsule.jpg"];

        foreach (var subDir in Directory.GetDirectories(appDir))
        {
            foreach (var name in preferredNames)
            {
                var candidate = Path.Combine(subDir, name);
                if (File.Exists(candidate)) return candidate;
            }
        }

        return null;
    }

    private static string? FindSteamRoot()
    {
        if (OperatingSystem.IsWindows())
        { 
            var regPath = GameScanHelper.ReadRegistry(@"Valve\Steam", "InstallPath");
            if (!string.IsNullOrWhiteSpace(regPath) && Directory.Exists(regPath))
                return regPath;

            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");
            if (Directory.Exists(defaultPath)) return defaultPath;
        }

        if (OperatingSystem.IsLinux())
        {
            var home      = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var linuxPaths = new[]
            {
                Path.Combine(home, ".steam", "steam"),
                Path.Combine(home, ".local", "share", "Steam"),
                Path.Combine(home, "snap", "steam", "common", ".local", "share", "Steam")

            };
            return linuxPaths.FirstOrDefault(Directory.Exists);
        }

        return null;
    }
    
    // GOG ---------------------------------------------------------------------------------

    private async Task<string?> TryGetGogLocalCover(Game game)
    {
        if (!OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(game.PlatformId)) return null;

        var productId  = StripPrefix(game.PlatformId, "gog:");
        var webCacheRoot  = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "GOG.com", "Galaxy", "webcache");
        
        if (!Directory.Exists(webCacheRoot)) return null;

        foreach (var guidDir in Directory.GetDirectories(webCacheRoot))
        {
            try
            {
                var imagePath = Path.Combine(guidDir, "gog", productId);
                if (!Directory.Exists(imagePath)) continue;

                var files = Directory.GetFiles(imagePath, "*.*")
                    .Where(f => f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => new FileInfo(f).Length)
                    .ToArray();
                
                if (files.Length == 0) continue;
                
                //Getting the middle one as it represent the cover instead of the icon or banner
                var found = files[files.Length / 2];
                
                await _logService.LogInfoAsync($"Found {game.Name} in guid: {guidDir} Product ID: {productId}");
                return found;
                
            }
            catch(Exception ex)
            {
                await _logService.LogErrorAsync($"Failed to get cover for [{game.Name}]",ex);
            }
            
        }

        return null;
    }

    private async Task<string?> TryResolveGogApiCoverAsync(Game game)
    {
        if (string.IsNullOrWhiteSpace(game.PlatformId)) return null;

        var productId = StripPrefix(game.PlatformId, "gog:");
        try
        {
            var url            = string.Format(GogApiV2ProductUrl, productId);
            using var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json      = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("_links", out var links)) return null;
            if (!links.TryGetProperty("coverImage", out var coverImage)) return null;
            if(!coverImage.TryGetProperty("href", out var href)) return null;

            var coverUrl = href.GetString();
            return string.IsNullOrWhiteSpace(coverUrl) ? null : coverUrl;

        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"GOG API cover lookup failed [{game.Name}]", ex);
            return null;
        }
    }

    // Heroic (GOG + Epic) -----------------------------------------------------------------

    private async Task<string?> TryResolveHeroicCoverAsync(Game game)
    {
        var heroicPath = GetHeroicConfigPath();
        if (heroicPath is null) return null;

        var isGog      = game.PlatformName == Game.Platform.GOG;
        var cacheFile  = Path.Combine(heroicPath, "store_cache",
            isGog ? "gog_library.json" : "legendary_library.json");

        if (!File.Exists(cacheFile)) return null;

        var gameId = isGog
            ? StripPrefix(game.PlatformId ?? string.Empty, "gog:")
            : StripPrefix(game.PlatformId ?? string.Empty, "epic:");
        var normalizedName = GameNameHelper.NormalizeName(game.Name ?? string.Empty);

        try
        {
            var json      = await File.ReadAllTextAsync(cacheFile);
            using var doc = JsonDocument.Parse(json);
            
            var rootKey = isGog ? "games" : "library";

            if (!doc.RootElement.TryGetProperty(rootKey, out var library)) return null;

            JsonElement? bestMatch = null;
            
            foreach (var entry in library.EnumerateArray())
            {
                if (entry.TryGetProperty("app_name", out var appName)
                    && string.Equals(appName.GetString(), gameId, StringComparison.OrdinalIgnoreCase))
                {
                    bestMatch = entry;
                    break;
                }
                
                if(bestMatch is null)
                {
                    var titleProp = entry.TryGetProperty("title", out var title) ? title.GetString() :
                        entry.TryGetProperty("app_title", out var appTitle) ? appTitle.GetString() : null;

                    if (!string.IsNullOrWhiteSpace(titleProp) &&
                        GameNameHelper.FuzzyNameMatch(normalizedName,
                            GameNameHelper.NormalizeName(titleProp)))
                        bestMatch = entry;
                }
            }
            if (bestMatch is null) return null;

            if (bestMatch.Value.TryGetProperty("art_square", out var artSquare) &&
                !string.IsNullOrWhiteSpace(artSquare.GetString()))
                return artSquare.GetString();

            if (bestMatch.Value.TryGetProperty("art_cover", out var artCover) &&
                !string.IsNullOrWhiteSpace(artCover.GetString()))
                return artCover.GetString();
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Heroic cover lookup failed [{game.Name}]", ex);
        }

        return null;
    }

    private static string? GetHeroicConfigPath()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "heroic");

        return OperatingSystem.IsLinux() ? 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "heroic") : null;
    }
    
    // PCGamingWiki (fallback) -------------------------------------------------------------

    private async Task<string?> ResolvePcgwCoverUrlAsync(Game game)
    {
        //Last resort for Steam CDN
        // if (game.PlatformName == Game.Platform.Steam &&
        //     !string.IsNullOrWhiteSpace(game.PlatformId))
        // {
        //     var appId = StripPrefix(game.PlatformId, "steam:");
        //     return await TryPcgwCargoAsync(
        //         string.Format(PcgwCargoByAppIdUrl, Uri.EscapeDataString(appId)));
        // }
        
        
        return await ResolvePcgwBySearchAsync(game.Name ?? string.Empty);
    }

    private async Task<string?> ResolvePcgwBySearchAsync(string gameName)
    {
        // Pass 1: exact page name match
        var exactUrl = await TryPcgwCargoAsync(
            string.Format(PcgwCargoByPageNameUrl, Uri.EscapeDataString(gameName)));
        if (exactUrl is not null) return exactUrl;

        // Pass 2: Media wiki search - handles fuzzy name match differences
        try
        {
            var searchApiUrl   = string.Format(PcgwSearchUrl, Uri.EscapeDataString(gameName));
            using var response = await _httpClient.GetAsync(searchApiUrl);
            if (!response.IsSuccessStatusCode) return null;

            var json   = await response.Content.ReadAsStringAsync();
            var pageId = ParseTopSearchPageId(json);
            if (pageId is null) return null;
            
            //Reading the cargoquery via ParseCargoCoverUrl
            return await TryPcgwCargoAsync(string.Format(PcgwCargoByPageIdUrl, pageId));
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"PCGamingWiki search failed [{gameName}]", ex);
            return null;
        }
    }

    private static string? ParseTopSearchPageId(string json)
    {
        //Reads the SearchUrl and selects the page id
        using var doc     = JsonDocument.Parse(json);
        var searchResults = doc.RootElement
            .GetProperty("query")
            .GetProperty("search");

        if (searchResults.GetArrayLength() == 0) return null;
        return searchResults[0].GetProperty("pageid").GetInt32().ToString();
    }

    private async Task<string?> TryPcgwCargoAsync(string apiUrl)
    {
        try
        {
            using var response = await _httpClient.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return ParseCargoCoverUrl(json);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"PCGamingWiki Cargo API failed [{apiUrl}]", ex);
            return null;
        }
    }

    private static string? ParseCargoCoverUrl(string json)
    {   //Handling the request and passing the JSON file
        using var doc = JsonDocument.Parse(json);
        var results   = doc.RootElement.GetProperty("cargoquery");
        if (results.GetArrayLength() == 0) return null;
        
        if (!results[0].GetProperty("title").TryGetProperty("Cover URL", out var coverUrlProp))
            return null;

        var coverUrl = coverUrlProp.GetString();
        return string.IsNullOrWhiteSpace(coverUrl) ? null : coverUrl;
    }

    // Shared -------------------------------------------------------------------------------
    
    //TODO: Remove since PlatformId was being used for SteamGridDbService and now when it's not being used.
    // I don't have to strip them down and just use var appId = game.PlatformId instead of game.PlatformId, "steam:"
    private static string StripPrefix(string value, string prefix) =>
        value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..]
            : value;

    
    private async Task<bool> TryDownloadCoverAsync(string? gameName, string coverPath, string url)
    {
        try
        {
            byte[] bytes;
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                bytes = await response.Content.ReadAsByteArrayAsync();
            }
            else if (!OperatingSystem.IsWindows() &&
                     ((int)response.StatusCode == 403 || (int)response.StatusCode == 503))
            {
                await _logService.LogInfoAsync
                    ($"HttpClient blocked by Cloudflare for [{gameName}] — retrying via curl");
                bytes = await DownloadViaCurlAsync(url);
            }
            else
            {
                return false;
            }

            if (bytes.Length == 0) return false;

            bytes = await _imageResizeService.ReSizeImageToWidthAsync(bytes, 600);

            await File.WriteAllBytesAsync(coverPath, bytes);
            await _logService.LogInfoAsync($"Downloaded cover for [{gameName}]");
            return true;
        }
        catch (HttpRequestException) when (!OperatingSystem.IsWindows())
        {
            await _logService.LogInfoAsync
                ($"HttpClient failed for [{gameName}] — retrying via curl");
            var bytes = await DownloadViaCurlAsync(url);
            if (bytes.Length == 0) return false;

            bytes = await _imageResizeService.ReSizeImageToWidthAsync(bytes, 600);

            await File.WriteAllBytesAsync(coverPath, bytes);
            await _logService.LogInfoAsync($"Downloaded cover for [{gameName}]");
            return true;
        }
        catch (HttpRequestException ex) when ((int?)ex.StatusCode == 404)
        {
            //Silent call
            return false;
        }
        catch (HttpRequestException ex) when ((int?)ex.StatusCode == 403)
        {
            await _logService.LogWarningAsync
                ($"403 Forbidden [{gameName}] — [{url}]");
            return false;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync
                ($"Failed to download cover for [{gameName}]", ex);
            return false;
        }
    }
    
    // Curl Download -------------------------------------------------------------------------------
    private async Task<byte[]> DownloadViaCurlAsync(string url)
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("curl")
            {
                Arguments              = $"--silent --fail --location --output \"{tempFile}\" --write-out \"%{{http_code}}\" \"{url}\"",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false
            };

            using var proc = System.Diagnostics.Process.Start(psi)
                             ?? throw new InvalidOperationException("curl is not available on this system. Install curl to enable cover downloads on Linux.");

            var statusCode = (await proc.StandardOutput.ReadToEndAsync()).Trim();
            await proc.WaitForExitAsync();

            if (proc.ExitCode is not 0)
            {
                var httpStatus = int.TryParse(statusCode, out var code) ? code : 0;
                throw new HttpRequestException($"curl exited with code {proc.ExitCode}",
                    null,
                    httpStatus > 0 ? (System.Net.HttpStatusCode?)httpStatus : null);
            }

            return await File.ReadAllBytesAsync(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    // Icons -----------------------------------------------------------------
    
    private async Task ExtractIconIfMissingAsync(string? executablePath, string? gameName, string iconPath)
    {
        if (File.Exists(iconPath)) return;
    
        var exePath = ResolveMainExecutablePath(executablePath, gameName);
        if (exePath is null) return;
    
        try
        {
            var iconBytes = await Task.Run(() => PeIconHelper.ExtractLargestIconAsPng(exePath));
            if (iconBytes is null) return;
    
            await File.WriteAllBytesAsync(iconPath, iconBytes);
            await _logService.LogInfoAsync($"Extracted icon for [{gameName}] to [{iconPath}]");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to extract icon for [{gameName}]", ex);
        }
    }
    
    private static string? ResolveMainExecutablePath(string? executablePath, string? gameName)
    {
        if (string.IsNullOrWhiteSpace(executablePath) || !Directory.Exists(executablePath))
            return null;
    
        var exes = Directory.GetFiles(executablePath, "*.exe")
            .Where(e => !GameScanHelper.NonGameExecutable(Path.GetFileNameWithoutExtension(e)))
            .ToArray();
    
        switch (exes.Length)
        {
            case 0: return null;
            case 1: return exes[0];
        }
    
        var normalized = GameNameHelper.NormalizeName(gameName ?? string.Empty);
        var stripped   = GameNameHelper.StripEditionSuffix(normalized);
    
        var match = exes.FirstOrDefault(e =>
        {
            var exeName = GameNameHelper.NormalizeName(Path.GetFileNameWithoutExtension(e));
            return exeName.Contains(normalized)                       ||
                   exeName.Contains(stripped)                         ||
                   normalized.Contains(exeName)                       ||
                   GameNameHelper.FuzzyNameMatch(normalized, exeName) ||
                   GameNameHelper.FuzzyNameMatch(stripped, exeName);
        });
    
        return match ?? exes.OrderByDescending(e => new FileInfo(e).Length).First();
    }
}