using HtmlAgilityPack;
using Restall.Application.DTOs;
using Restall.Application.Helpers;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Services;

public class ParseService : IParseService
{
    private readonly ILogService _logService;
    private readonly HttpClient _httpClient;

    private const string ReShadeTagsUrl = "https://github.com/crosire/reshade/tags";
    private const string ReShadeSiteUrl = "https://reshade.me";

    private const string RenoDxUrl = "https://github.com/clshortfuse/renodx/wiki/Mods/";
    private const string RenoDxTagUrl = "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd
    
    private readonly Dictionary<ReShade.Branch, List<string>> _availableReShadeVersions = [];
    private readonly Dictionary<RenoDX.Branch, List<RenoDXTagInfoDto>> _availableRenoDxTags = [];
    // Store by mod name
    private readonly Dictionary<string, List<RenoDXModInfoDto>> _availableWikiModsByName = [];
    private readonly Dictionary<string, RenoDXModPreferenceDto> _modPreferences = [];

    public ParseService(
        ILogService logService,
        HttpClient httpClient
        )
    {
        _logService = logService;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Restall");
    }
    
    public async Task FetchAvailableModVersionsAsync()
    {
        await Task.WhenAll(
            FetchStableReShadeVersionsAsync(),
            FetchLatestRenoDxSnapshotAsync(),
            FetchRenoDxNightlyVersionsAsync(),
            FetchRenoDxWikiModsAsync()
        );
    }

    public RenoDXModInfoDto? GetCompatibleRenoDXMod(string? gameName)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return null;

        var key = GameNameHelper.NormalizeName(gameName);

        if (_availableWikiModsByName.TryGetValue(key, out var mods))
            return mods.FirstOrDefault(m => m.Status != "💀");

        var fallback = _availableWikiModsByName
            .FirstOrDefault(kv => kv.Key.Contains(key) || key.Contains(kv.Key));

        return fallback.Value?.FirstOrDefault(m => m.Status != "💀");
    }

    private async Task FetchStableReShadeVersionsAsync()
    {
        try
        {
            var versions = await FetchReShadeVersionsFromGitHubTagsAsync();
            var siteVersion = await FetchLatestReShadeVersionFromSiteAsync();

            if (siteVersion is not null && !versions.Contains(siteVersion))
            {
                versions.Insert(0, siteVersion);
                await _logService.LogInfoAsync($"reshade.me has a newer version not yet on GitHub tags: {siteVersion}");
            }

            _availableReShadeVersions[ReShade.Branch.Stable] = versions;
            await _logService.LogInfoAsync($"Fetched {versions.Count} stable ReShade versions. Latest: {versions.FirstOrDefault()}");
        }
        catch ( Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch stable ReShade versions.", ex);
        }
    }

    private async Task<string?> FetchLatestReShadeVersionFromSiteAsync()
    {
        try
        {
            var html = await _httpClient.GetStringAsync(ReShadeSiteUrl);
            var match = Regex.Match(html, @"ReShade (\d+\.\d+\.\d+)");
            return match.Success ? match.Groups[1].Value : null;
        }
        catch ( Exception ex)
        {
            await _logService.LogErrorAsync("Could not fetch latest ReShade version from reshade.me.", ex);
            return null;
        }
    }

    private async Task<List<string>> FetchReShadeVersionsFromGitHubTagsAsync()
    {
        var versions = new List<string>();

        try
        {
            var html = await _httpClient.GetStringAsync(ReShadeTagsUrl);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var tagNodes = document.DocumentNode
                .SelectNodes("//a[contains(@href, 'crosire/reshade/releases/tag/')]");

            if (tagNodes is null)
            {
                await _logService.LogWarningAsync("No ReShade tags found to parse.");
                return versions;
            }

            foreach (var node in tagNodes)
            {
                var href = node.GetAttributeValue("href", string.Empty);
                var tag = href.Split('/').LastOrDefault();

                if (string.IsNullOrWhiteSpace(tag)) continue;

                var version = tag.TrimStart('v');

                if (!string.IsNullOrWhiteSpace(version) && !versions.Contains(version))
                    versions.Add(version);

                await _logService.LogInfoAsync($"Found ReShade version: {version}");
            }
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Unable to parse ReShade GitHub tags page.", ex);
        }

        return versions;
    }

    private async Task FetchRenoDxWikiModsAsync()
    {
        try
        {
            var html = await _httpClient.GetStringAsync(RenoDxUrl);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var rows = document.DocumentNode.SelectNodes("//table[1]//tr[position() > 1]");

            if (rows is null)
            {
                await _logService.LogWarningAsync("No rows found in RenoDX wiki.");
                return;
            }

            foreach (var row in rows)
            {
                var cells = row.SelectNodes("./td");
                if (cells is null || cells.Count < 4) continue;

                var rawName = HtmlEntity.DeEntitize(cells[0].InnerText).Trim();
                if (string.IsNullOrWhiteSpace(rawName)) continue;

                var maintainer = HtmlEntity.DeEntitize(cells[1].InnerText).Trim();
                if (string.IsNullOrWhiteSpace(maintainer))
                    maintainer = "Unknown";

                var linksCell = cells[2];
                var statusRaw = cells[3].InnerText.Trim();

                var snapshotNode64 = linksCell.SelectSingleNode(".//a[contains(@href, '.addon64')]");
                var snapshotNode32 = linksCell.SelectSingleNode(".//a[contains(@href, '.addon32')]");
                var nexusNode      = linksCell.SelectSingleNode(".//a[contains(@href, 'nexusmods.com')]");
                var discordNode    = linksCell.SelectSingleNode(".//a[contains(@href, 'discord')]");

                var mod = new RenoDXModInfoDto(
                    Name:           rawName,
                    DiscordUrl:     discordNode?.GetAttributeValue("href", null),
                    SnapshotUrl64:  snapshotNode64?.GetAttributeValue("href", null),
                    SnapshotUrl32:  snapshotNode32?.GetAttributeValue("href", null),
                    NexusUrl:       nexusNode?.GetAttributeValue("href", null),
                    Maintainer:     maintainer,
                    Notes:          null,
                    Status:         statusRaw
                    );

                var key = GameNameHelper.NormalizeName(rawName);

                if (!_availableWikiModsByName.TryGetValue(key, out var list))
                {
                    list = [];
                    _availableWikiModsByName[key] = list;
                }

                list.Add(mod);
            }

            await _logService.LogInfoAsync($"Parsed {_availableWikiModsByName.Count} RenoDX compatible games from wiki.");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch RenoDX wiki mods.", ex);
        }
    }
    
    private async Task FetchLatestRenoDxSnapshotAsync()
    {
        try
        {
            var html = await _httpClient.GetStringAsync(RenoDxTagUrl + "snapshot");
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var timeNode = document.DocumentNode.SelectSingleNode("//relative-time");
            DateOnly? date = null;
            if (timeNode is not null)
            {
                var datetime = timeNode.GetAttributeValue("datetime", null);
                if (DateTime.TryParse(datetime, out var parsed))
                    date = DateOnly.FromDateTime(parsed.ToUniversalTime());
            }

            if (date is null)
            {
                await _logService.LogWarningAsync("Failed to parse snapshot release date");
                return;
            }

            var bodyNode = document.DocumentNode.SelectSingleNode(
                "//div[contains(@class, 'markdown-body')]");
            var commitNotes = new List<string>();

            if (bodyNode is not null)
            {
                string? currentSection = null;
                foreach (var node in bodyNode.ChildNodes)
                {
                    if (node.Name == "h2")
                    {
                        currentSection = node.InnerText.Trim();
                        continue;
                    }

                    if (node.Name == "ul")
                    {
                        foreach (var li in node.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())
                        {
                            var text = li.InnerText.Trim();
                            if (!string.IsNullOrWhiteSpace(text))
                                commitNotes.Add(currentSection is not null ? $"[{currentSection}] {text}" : text);
                        }
                    }
                }
            }
            
            await _logService.LogInfoAsync($"Successfully parsed RenoDX snapshot: {date.Value}\n{string.Join(Environment.NewLine, commitNotes)}");
            _availableRenoDxTags[RenoDX.Branch.Snapshot] = [new RenoDXTagInfoDto(date.Value, RenoDX.Branch.Snapshot, commitNotes)];
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch latest RenoDX snapshot", ex);
        }
    }

    private async Task FetchRenoDxNightlyVersionsAsync()
    {
        throw new System.NotImplementedException();
    }
}