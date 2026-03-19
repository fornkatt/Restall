using HtmlAgilityPack;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

internal sealed class ParseService : IParseService
{
    private readonly ILogService _logService;
    private readonly IHttpClientFactory _httpClientFactory;
    private HttpClient HttpClient => _httpClientFactory.CreateClient("ParseService");

    private const string s_reShadeTagsUrl = "https://github.com/crosire/reshade/tags";
    private const string s_reShadeSiteUrl = "https://reshade.me";

    private const string s_renoDxUrl = "https://raw.githubusercontent.com/wiki/clshortfuse/renodx/Mods.md";
    private const string s_renoDxTagsUrl = "https://github.com/clshortfuse/renodx/tags";
    private const string s_renoDxReleasesTagUrl = "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd

    public ParseService(
        ILogService logService,
        IHttpClientFactory httpClientFactory
        )
    {
        _logService = logService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<string>> FetchReShadeVersionsAsync()
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

            await _logService.LogInfoAsync($"Fetched {versions.Count} stable ReShade versions. Latest: {versions.FirstOrDefault()}");
            return versions;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Unexpected error occured during parsing. Failed to fetch stable ReShade versions.", ex);
            return [];
        }
    }

    public async Task<RenoDXTagInfoDto?> FetchRenoDXSnapshotAsync()
    {
        try
        {
            var document = await LoadHtmlDocumentAsync(s_renoDxReleasesTagUrl + "snapshot");

            var timeNode = document.DocumentNode.SelectSingleNode("//relative-time");
            DateOnly? date = null;
            if (timeNode is not null)
            {
                var datetime = timeNode.GetAttributeValue("datetime", string.Empty);
                if (DateTime.TryParse(datetime, out var parsed))
                    date = DateOnly.FromDateTime(parsed.ToUniversalTime());
            }

            if (date is null)
            {
                await _logService.LogWarningAsync("Failed to parse snapshot release date");
                return null;
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
            return new RenoDXTagInfoDto(date.Value, RenoDX.Branch.Snapshot, commitNotes);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch latest RenoDX snapshot", ex);
            return null;
        }
    }

    public async Task<IReadOnlyList<RenoDXTagInfoDto>> FetchRenoDXNightlyTagsAsync()
    {
        try
        {
            var nightlyTags = await FetchRenoDXNightlyTagNamesAsync();

            if (nightlyTags.Count <= 0)
            {
                await _logService.LogWarningAsync("No nightly RenoDX tags found.");
                return [];
            }

            var tagInfoResults = await Task.WhenAll(nightlyTags.Select(FetchRenoDXNighlyReleaseInfoAsync));
            var tagInfos = tagInfoResults.OfType<RenoDXTagInfoDto>().ToList();

            await _logService.LogInfoAsync($"Fetched {tagInfos.Count} nightly RenoDX versions. " +
                $"Latest: {tagInfos.FirstOrDefault()?.Version}");
            return tagInfos;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch nightly RenoDX versions.", ex);
            return [];
        }
    }

    public async Task<RenoDXWikiParseResultDto> FetchRenoDXWikiModsAsync()
    {
        var wikiMods = new List<RenoDXModInfoDto>();
        var genericWikiMods = new List<RenoDXGenericModInfoDto>();

        try
        {
            var markdown = await HttpClient.GetStringAsync(s_renoDxUrl);
            var lines = markdown.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            SupportedEngine? currentEngine = null;
            bool inTable = false;
            bool headerSkipped = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (line.StartsWith("### Unreal Engine", StringComparison.OrdinalIgnoreCase))
                {
                    currentEngine = SupportedEngine.Unreal;
                    inTable = false;
                    headerSkipped = false;
                    continue;
                }
                if (line.StartsWith("### Unity Engine", StringComparison.OrdinalIgnoreCase))
                {
                    currentEngine = SupportedEngine.Unity;
                    inTable = false;
                    headerSkipped = false;
                    continue;
                }
                if (line.StartsWith('#'))
                {
                    currentEngine = null;
                    inTable = false;
                    headerSkipped = false;
                    continue;
                }

                if (!line.StartsWith('|') || line.StartsWith("| ---") || line.StartsWith("|---"))
                    continue;

                if (!inTable)
                    inTable = true;

                if (!headerSkipped)
                {
                    headerSkipped = true;
                    continue;
                }

                var cells = line.Split('|', StringSplitOptions.RemoveEmptyEntries);

                if (currentEngine is not null)
                {
                    if (cells.Length < 2) continue;
                    var name = ExtractMarkdownLinkText(HtmlEntity.DeEntitize(cells[0].Trim()));
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    var status = cells[1].Trim();
                    var notes = cells.Length >= 3 ? cells[2].Trim() : null;
                    if (string.IsNullOrWhiteSpace(notes))
                        notes = null;

                    genericWikiMods.Add(new RenoDXGenericModInfoDto(
                        Name: name,
                        Status: status,
                        Notes: notes,
                        Engine: currentEngine.Value
                        ));
                }
                else
                {
                    if (cells.Length < 4) continue;
                    var name = ExtractMarkdownLinkText(HtmlEntity.DeEntitize(cells[0].Trim()));
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    var maintainer = cells[1].Trim();
                    if (string.IsNullOrWhiteSpace(maintainer))
                        maintainer = "Unknown";
                    var linksCell = cells[2];
                    var status = cells[3].Trim();

                    wikiMods.Add(new RenoDXModInfoDto(
                    Name: name,
                    DiscordUrl: ExtractMarkdownUrl(linksCell, "discord"),
                    SnapshotUrl64: ExtractMarkdownUrl(linksCell, ".addon64"),
                    SnapshotUrl32: ExtractMarkdownUrl(linksCell, ".addon32"),
                    NexusUrl: ExtractMarkdownUrl(linksCell, "nexusmods.com"),
                    Maintainer: maintainer,
                    Notes: null,
                    Status: status
                    ));
                }
            }

            await _logService.LogInfoAsync($"Parsed {genericWikiMods.Count} generic RenoDX mods from wiki.");
        }
        catch (HttpRequestException ex)
        {
            await _logService.LogErrorAsync($"RenoDX GitHub wiki page is unreachable. ({(int?)ex.StatusCode})", ex);
        }
        catch (TaskCanceledException ex)
        {
            await _logService.LogErrorAsync($"Request for RenoDX wiki page timed out.", ex);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch RenoDX wiki mods.", ex);
        }

        return new RenoDXWikiParseResultDto(wikiMods, genericWikiMods);
    }

    private static string ExtractMarkdownLinkText(string text)
    {
        var bracketEnd = text.IndexOf("](", StringComparison.Ordinal);
        if (bracketEnd < 0) return text;

        var bracketStart = text.LastIndexOf('[', bracketEnd);
        if (bracketStart < 0) return text;

        return text[(bracketStart + 1)..bracketEnd].Trim();
    }

    private string? ExtractMarkdownUrl(string markdown, string urlContains)
    {
        var start = 0;

        while (true)
        {
            int linkOpen = markdown.IndexOf("](", start, StringComparison.Ordinal);
            if (linkOpen < 0) return null;
            int urlStart = linkOpen + 2;
            int urlEnd = markdown.IndexOf(')', urlStart);
            if (urlEnd < 0) return null;
            var url = markdown[urlStart..urlEnd];
            if (url.Contains(urlContains, StringComparison.OrdinalIgnoreCase))
                return url;
            start = urlEnd + 1;
        }
    }

    private async Task<string?> FetchLatestReShadeVersionFromSiteAsync()
    {
        try
        {
            var document = await HttpClient.GetStringAsync(s_reShadeSiteUrl);
            var match = RegexHelper.ExtractReShadeVersionFromSite.Match(document);
            return match.Success ? match.Groups[1].Value : null;
        }
        catch (Exception ex)
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
            var document = await LoadHtmlDocumentAsync(s_reShadeTagsUrl);

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
        catch (HttpRequestException ex)
        {
            await _logService.LogErrorAsync($"GitHub tags page for ReShade is unreachable. ({(int?)ex.StatusCode})", ex);
            return versions;
        }
        catch (TaskCanceledException ex)
        {
            await _logService.LogErrorAsync("GitHub tags page for ReShade timed out.", ex);
            return versions;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Unable to parse ReShade GitHub tags page. " +
                "Page structure likely changed or other unexpected error.", ex);
            return versions;
        }

        return versions;
    }

    private async Task<List<string>> FetchRenoDXNightlyTagNamesAsync(string? pageUrl = null)
    {
        var tags = new List<string>();

        try
        {
            var document = await LoadHtmlDocumentAsync(pageUrl ?? s_renoDxTagsUrl);

            var tagNodes = document.DocumentNode
                .SelectNodes("//a[contains(@href, 'clshortfuse/renodx/releases/tag/nightly-')]");

            if (tagNodes is null)
            {
                await _logService.LogWarningAsync("No nightly tag links found on RenoDX tags page.");
                return tags;
            }

            foreach (var node in tagNodes)
            {
                var href = node.GetAttributeValue("href", string.Empty);
                var tag = href.Split('/').LastOrDefault();

                if (string.IsNullOrWhiteSpace(tag) || !tag.StartsWith("nightly-")) continue;
                if (!tags.Contains(tag))
                    tags.Add(tag);
            }
        }
        catch (HttpRequestException ex)
        {
            await _logService.LogErrorAsync($"GitHub tags page for RenoDX is unreachable. ({(int?)ex.StatusCode})", ex);
            return tags;
        }
        catch (TaskCanceledException ex)
        {
            await _logService.LogErrorAsync("GitHub tags page for RenoDX timed out.", ex);
            return tags;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Unable to parse RenoDX GitHub tags page." +
                "Page structure might have changed or other unextected error.", ex);
            return tags;
        }

        return tags;
    }

    private async Task<RenoDXTagInfoDto?> FetchRenoDXNighlyReleaseInfoAsync(string nightlyTag)
    {
        try
        {
            var dateStr = nightlyTag["nightly-".Length..];
            if (!DateOnly.TryParseExact(dateStr, "yyyyMMdd", null,
                System.Globalization.DateTimeStyles.None, out var date))
            {
                await _logService.LogWarningAsync($"Could not parse date from nightly tag: {nightlyTag}");
                return null;
            }

            var document = await LoadHtmlDocumentAsync(s_renoDxReleasesTagUrl + nightlyTag);

            var preNode = document.DocumentNode
                .SelectSingleNode("//pre[contains(@class, 'text-small') and contains(@class, 'ws-pre-wrap')]");

            List<string>? commitNotes = null;

            if (preNode is not null)
            {
                var lines = HtmlEntity.DeEntitize(preNode.InnerText)
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Skip(1)
                    .ToList();

                if (lines.Count > 0)
                    commitNotes = lines;
            }
            else
            {
                await _logService.LogInfoAsync($"No release notes found for {nightlyTag}.");
            }

            await _logService.LogInfoAsync($"Parsed RenoDX nightly: {nightlyTag}" +
                $"{(commitNotes is not null ? $"\n{string.Join(Environment.NewLine, commitNotes)}" : string.Empty)}");

            return new RenoDXTagInfoDto(date, RenoDX.Branch.Nightly, commitNotes);
        }
        catch (HttpRequestException ex)
        {
            await _logService.LogErrorAsync($"Page for {nightlyTag} is unreachable. ({(int?)ex.StatusCode})", ex);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            await _logService.LogErrorAsync($"Request timed out while fetching {nightlyTag}", ex);
            return null;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to fetch release info for {nightlyTag}." +
                $"Page structure might have changed or other unexpected error.", ex);
            return null;
        }
    }

    private async Task<HtmlDocument> LoadHtmlDocumentAsync(string url)
    {
        await using var stream = await HttpClient.GetStreamAsync(url);
        var document = new HtmlDocument();
        document.Load(stream);
        return document;
    }
}