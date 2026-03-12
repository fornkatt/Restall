using HtmlAgilityPack;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

public class ParseService : IParseService
{
    private readonly ILogService _logService;
    private readonly HttpClient _httpClient;

    private const string s_reShadeTagsUrl = "https://github.com/crosire/reshade/tags";
    private const string s_reShadeSiteUrl = "https://reshade.me";

    private const string s_renoDxUrl = "https://github.com/clshortfuse/renodx/wiki/Mods/";
    private const string s_renoDxTagsUrl = "https://github.com/clshortfuse/renodx/tags";
    private const string s_renoDxReleasesTagUrl = "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd

    private readonly Dictionary<ReShade.Branch, List<string>> _availableReShadeVersions = [];
    private readonly Dictionary<RenoDX.Branch, List<RenoDXTagInfoDto>> _availableRenoDXTags = [];

    public ParseService(
        ILogService logService,
        HttpClient httpClient
        )
    {
        _logService = logService;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Restall");
    }

    public RenoDXTagInfoDto? GetLatestRenoDXTag(RenoDX.Branch branch) =>
        _availableRenoDXTags.TryGetValue(branch, out var tags) ? tags.FirstOrDefault() : null;

    public IReadOnlyList<RenoDXTagInfoDto> GetAllRenoDXNightlies() =>
        _availableRenoDXTags.TryGetValue(RenoDX.Branch.Nightly, out var tags) ? tags.AsReadOnly() : [];

    public IReadOnlyList<string> GetAvailableReShadeVersions(ReShade.Branch branch) =>
        _availableReShadeVersions.TryGetValue(branch, out var versions) ? versions.AsReadOnly() : [];

    public string? GetLatestReShadeVersion(ReShade.Branch branch) =>
        _availableReShadeVersions.TryGetValue(branch, out var versions) ? versions.FirstOrDefault() : null;

    public async Task<WikiParseResultDto> FetchAvailableModsAsync()
    {
        var versionTask = Task.WhenAll(
            FetchStableReShadeVersionsAsync(),
            FetchLatestRenoDxSnapshotAsync(),
            FetchRenoDxNightlyVersionsAsync()
        );

        var wikiTask = FetchRenoDXWikiModsAsync();

        await Task.WhenAll(versionTask, wikiTask);

        var (wikiMods, genericWikiMods) = wikiTask.Result;

        return new WikiParseResultDto(wikiMods, genericWikiMods);
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
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch stable ReShade versions.", ex);
        }
    }

    private async Task<string?> FetchLatestReShadeVersionFromSiteAsync()
    {
        try
        {
            var document = await _httpClient.GetStringAsync(s_reShadeSiteUrl);
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
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Unable to parse ReShade GitHub tags page.", ex);
        }

        return versions;
    }

    private async Task<(List<RenoDXModInfoDto> WikiMods, List<RenoDXGenericModInfoDto> GenericWikiMods)> FetchRenoDXWikiModsAsync()
    {
        var wikiMods = new List<RenoDXModInfoDto>();
        var genericWikiMods = new List<RenoDXGenericModInfoDto>();

        try
        {
            var document = await LoadHtmlDocumentAsync(s_renoDxUrl);

            var rows = document.DocumentNode.SelectNodes("//table[1]//tr[position() > 1]");

            if (rows is null)
            {
                await _logService.LogWarningAsync("No rows found in RenoDX wiki.");
                return (wikiMods, genericWikiMods);
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
                var nexusNode = linksCell.SelectSingleNode(".//a[contains(@href, 'nexusmods.com')]");
                var discordNode = linksCell.SelectSingleNode(".//a[contains(@href, 'discord')]");

                var mod = new RenoDXModInfoDto(
                    Name: rawName,
                    DiscordUrl: discordNode?.GetAttributeValue("href", string.Empty),
                    SnapshotUrl64: snapshotNode64?.GetAttributeValue("href", string.Empty),
                    SnapshotUrl32: snapshotNode32?.GetAttributeValue("href", string.Empty),
                    NexusUrl: nexusNode?.GetAttributeValue("href", string.Empty),
                    Maintainer: maintainer,
                    Notes: null,
                    Status: statusRaw
                    );

                wikiMods.Add(mod);
            }

            await _logService.LogInfoAsync($"Parsed {wikiMods.Count} RenoDX compatible games from wiki.");

            await ParseRenoDXGenericModTableAsync(document, "Unreal Engine", Engine.Unreal, genericWikiMods);
            await ParseRenoDXGenericModTableAsync(document, "Unity Engine", Engine.Unity, genericWikiMods);

            await _logService.LogInfoAsync($"Parsed {genericWikiMods.Count} generic RenoDX mods from wiki.");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch RenoDX wiki mods.", ex);
        }

        return (wikiMods, genericWikiMods);
    }

    private async Task ParseRenoDXGenericModTableAsync(HtmlDocument document,
                                                       string engineHeading,
                                                       Engine engine,
                                                       List<RenoDXGenericModInfoDto> genericWikiMods)
    {
        var h3Nodes = document.DocumentNode.SelectNodes("//h3");
        var targetH3 = h3Nodes?.FirstOrDefault(n => n.InnerText.Contains(engineHeading));

        if (targetH3 is null)
        {
            await _logService.LogWarningAsync($"Could not find {engineHeading} in RenoDX wiki.");
            return;
        }

        var anchorNode = targetH3.ParentNode.GetAttributeValue("class", string.Empty)
            .Contains("markdown-heading")
            ? targetH3.ParentNode
            : targetH3;

        static bool IsHeadingElement(HtmlNode n) =>
            n.Name is "h2" or "h3" ||
            (n.Name == "div" && n.GetAttributeValue("class", string.Empty).Contains("markdown-heading"));

        var tableNode = anchorNode.ParentNode.ChildNodes
            .SkipWhile(n => n != anchorNode)
            .Skip(1)
            .TakeWhile(n => !IsHeadingElement(n))
            .FirstOrDefault(n => n.Name == "table");

        if (tableNode is null)
        {
            await _logService.LogWarningAsync($"No table found after {engineHeading} in RenoDX wiki.");
            return;
        }

        var rows = tableNode.SelectNodes(".//tr[position() > 1]");

        if (rows is null)
        {
            await _logService.LogWarningAsync($"No rows found in {engineHeading} RenoDX mod table.");
            return;
        }

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("./td");
            if (cells is null || cells.Count < 2) continue;

            var rawName = HtmlEntity.DeEntitize(cells[0].InnerText).Trim();
            if (string.IsNullOrWhiteSpace(rawName)) continue;

            var statusRaw = cells[1].InnerText.Trim();

            var notes = cells.Count >= 3
                ? HtmlEntity.DeEntitize(cells[2].InnerText).Trim()
                : null;
            if (string.IsNullOrWhiteSpace(notes)) notes = null;

            genericWikiMods.Add(new RenoDXGenericModInfoDto(
                Name: rawName,
                Status: statusRaw,
                Notes: notes,
                Engine: engine
                ));
        }
    }

    private async Task FetchLatestRenoDxSnapshotAsync()
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
            _availableRenoDXTags[RenoDX.Branch.Snapshot] = [new RenoDXTagInfoDto(date.Value, RenoDX.Branch.Snapshot, commitNotes)];
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch latest RenoDX snapshot", ex);
        }
    }

    private async Task FetchRenoDxNightlyVersionsAsync()
    {
        try
        {
            var nightlyTags = await FetchRenoDXNightlyTagNamesAsync();

            if (nightlyTags.Count <= 0)
            {
                await _logService.LogWarningAsync("No nightly RenoDX tags found.");
                return;
            }

            var tagInfoResults = await Task.WhenAll(nightlyTags.Select(FetchRenoDXNighlyReleaseInfoAsync));

            var tagInfos = tagInfoResults.OfType<RenoDXTagInfoDto>().ToList();
            _availableRenoDXTags[RenoDX.Branch.Nightly] = tagInfos;

            await _logService.LogInfoAsync($"Fetched {tagInfos.Count} nightly RenoDX versions. " +
                $"Latest: {tagInfos.FirstOrDefault()?.Version}");
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch nightly RenoDX versions.", ex);
        }
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
        catch (Exception ex)
        {
            await _logService.LogErrorAsync("Failed to fetch RenoDX tags page", ex);
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
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to fetch release info for {nightlyTag}", ex);
            return null;
        }
    }

    private async Task<HtmlDocument> LoadHtmlDocumentAsync(string url)
    {
        await using var stream = await _httpClient.GetStreamAsync(url);
        var document = new HtmlDocument();
        document.Load(stream);
        return document;
    }
}