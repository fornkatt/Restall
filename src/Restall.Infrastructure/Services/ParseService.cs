using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Services;

internal sealed partial class ParseService : IParseService
{
    private const string s_reShadeTagsUrl = "https://github.com/crosire/reshade/tags";
    private const string s_reShadeSiteUrl = "https://reshade.me";

    private const string s_renoDxUrl = "https://raw.githubusercontent.com/wiki/clshortfuse/renodx/Mods.md";
    private const string s_renoDXTagsUrl = "https://github.com/clshortfuse/renodx/tags";

    private const string
        s_renoDXReleasesTagUrl =
            "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ParseService> _logger;

    [GeneratedRegex("""^\[(?<icon>:[\w_]+:)\]\(#\s*"(?<title>[^"]*)"\)$""")]
    private static partial Regex StatusHoverRegex();

    [GeneratedRegex(@"^>\s*\[!(?<kind>NOTE|WARNING|IMPORTANT|TIP|CAUTION)\]\s*$")]
    private static partial Regex GitHubAlertRegex();

    [GeneratedRegex(@"\*\*(.*?)\*\*")]
    private static partial Regex BoldRegex();

    [GeneratedRegex(@"\[!\[[^\]]*\]\([^)]*\)\]\([^)]*\)")]
    private static partial Regex BadgeLinkRegex();

    [GeneratedRegex(@"!\[[^\]]*\]\([^)]*\)")]
    private static partial Regex MarkdownImageRegex();

    [GeneratedRegex(@"\[([^\]]*)\]\([^)]*\)")]
    private static partial Regex MarkdownLinkRegex();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex ExtraWhitespaceRegex();

    public ParseService(
        ILogger<ParseService> logger,
        IHttpClientFactory httpClientFactory
    )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient HttpClient => _httpClientFactory.CreateClient("ParseService");

    // TODO: need better catch safety, global exception handler?
    public async Task<ImmutableArray<string>> FetchReShadeVersionsAsync()
    {
        LogReShadeVersionFetchStart(s_reShadeSiteUrl, s_reShadeTagsUrl);

        var versions = await FetchReShadeVersionsFromGitHubTagsAsync();
        var siteVersion = await FetchLatestReShadeVersionFromSiteAsync();

        if (siteVersion is not null && !versions.Contains(siteVersion))
        {
            versions.Insert(0, siteVersion);
            LogReShadeSiteVersionNewer(s_reShadeSiteUrl, s_reShadeTagsUrl, siteVersion);
        }

        if (_logger.IsEnabled(LogLevel.Information))
            LogReShadeVersionFetchFinished(versions.Count, versions.FirstOrDefault());

        return [.. versions];
    }

    // TODO: surface Result<T>
    public async Task<RenoDXTagInfoDto?> FetchRenoDXSnapshotAsync()
    {
        const string renoDXSnapshotUrl = s_renoDXReleasesTagUrl + "snapshot";

        LogRenoDXSnapshotFetchStart(renoDXSnapshotUrl);

        try
        {
            var document = await LoadHtmlDocumentAsync(renoDXSnapshotUrl);

            var timeNode = document.DocumentNode.SelectSingleNode("//relative-time");

            DateOnly? date = null;
            var datetime = string.Empty;

            if (timeNode is not null)
            {
                datetime = timeNode.GetAttributeValue("datetime", string.Empty);
                if (DateTime.TryParse(datetime, out var parsed))
                    date = DateOnly.FromDateTime(parsed.ToUniversalTime());
            }

            if (date is null)
            {
                LogRenoDXSnapshotReleaseDateParseFailure(datetime);
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
                        foreach (var li in node.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())
                        {
                            var text = li.InnerText.Trim();
                            if (!string.IsNullOrWhiteSpace(text))
                                commitNotes.Add(currentSection is not null ? $"[{currentSection}] {text}" : text);
                        }
                }
            }

            LogRenoDXSnapshotFetchSuccess(date.Value);

            if (_logger.IsEnabled(LogLevel.Debug))
                LogRenoDXSnapshotFetchCommitNotes(string.Join(Environment.NewLine, commitNotes));

            return new RenoDXTagInfoDto(date.Value, RenoDX.Branch.Snapshot, commitNotes);
        }
        catch (HttpRequestException ex)
        {
            LogSiteUnreachable(renoDXSnapshotUrl, ex.StatusCode, ex);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            LogSiteTimeout(renoDXSnapshotUrl, ex);
            return null;
        }
    }

    // TODO: surface Result<T>
    public async Task<ImmutableArray<RenoDXTagInfoDto>> FetchRenoDXNightlyTagsAsync()
    {
        LogRenoDXNightlyVersionsFetchStart(s_renoDXTagsUrl);

        var nightlyTags = await FetchRenoDXNightlyTagNamesAsync();

        if (nightlyTags.Length <= 0)
        {
            LogRenoDXNightlyVersionsNotFound();
            return [];
        }

        var tagInfoResults = await Task.WhenAll(nightlyTags.Select(FetchRenoDXNightlyReleaseInfoAsync));
        var tagInfos = tagInfoResults.OfType<RenoDXTagInfoDto>().ToImmutableArray();

        LogRenoDXNightlyVersionsFetched(tagInfos.Length, tagInfos.FirstOrDefault()?.Version);

        return tagInfos;
    }

    // TODO: surface Result<T>
    public async Task<RenoDXWikiParseResultDto> FetchRenoDXWikiModsAsync()
    {
        LogRenoDXWikiModsFetchStart(s_renoDxUrl);

        var skippedCount = 0;

        var wikiMods = new List<RenoDXModInfoDto>();
        var genericWikiMods = new List<RenoDXGenericModInfoDto>();
        var engineNotes = new Dictionary<RenoDXWikiModType, List<string>>();

        try
        {
            var markdown = await HttpClient.GetStringAsync(s_renoDxUrl);
            var lines = markdown.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            RenoDXWikiModType? currentEngine = null;
            RenoDXWikiModType? capturingNotesFor = null;
            var inTable = false;
            var headerSkipped = false;
            var inCodeFence = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (inCodeFence)
                {
                    if (capturingNotesFor is not null)
                        AddNoteLine(engineNotes, capturingNotesFor.Value, line.StartsWith("```")
                            ? line
                            : rawLine.TrimEnd('\r'));
                    if (line.StartsWith("```"))
                        inCodeFence = false;
                    continue;
                }

                if (line.StartsWith("# Deprecated mods")) break;

                if (line.StartsWith("### Unreal Engine", StringComparison.OrdinalIgnoreCase))
                {
                    currentEngine = RenoDXWikiModType.Unreal;
                    capturingNotesFor = currentEngine;
                    inTable = false;
                    headerSkipped = false;
                    inCodeFence = false;
                    continue;
                }

                if (line.StartsWith("### UE Extended", StringComparison.OrdinalIgnoreCase))
                {
                    currentEngine = RenoDXWikiModType.UnrealExtended;
                    capturingNotesFor = currentEngine;
                    inTable = false;
                    headerSkipped = false;
                    inCodeFence = false;
                    continue;
                }

                if (line.StartsWith("### Unity Engine", StringComparison.OrdinalIgnoreCase))
                {
                    currentEngine = RenoDXWikiModType.Unity;
                    capturingNotesFor = currentEngine;
                    inTable = false;
                    headerSkipped = false;
                    inCodeFence = false;
                    continue;
                }

                if (line.StartsWith('#'))
                {
                    currentEngine = null;
                    capturingNotesFor = null;
                    inTable = false;
                    headerSkipped = false;
                    inCodeFence = false;
                    continue;
                }

                if (capturingNotesFor is not null && !inTable && !line.StartsWith('|'))
                {
                    if (line.StartsWith("```"))
                    {
                        AddNoteLine(engineNotes, capturingNotesFor.Value, line);
                        inCodeFence = true;
                        continue;
                    }

                    if (BadgeLinkRegex().IsMatch(line))
                        continue;

                    AddNoteLine(engineNotes, capturingNotesFor.Value, CleanNotesLine(line));
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

                var cells = line.Trim('|').Split('|');

                if (currentEngine is not null)
                {
                    var architecture = Architecture.x64;

                    if (cells.Length < 2)
                    {
                        LogRenoDXSkipMalformedWikiModRow("at least 2", cells.Length, line);
                        skippedCount++;
                        continue;
                    }

                    var name = ExtractMarkdownLinkText(HtmlEntity.DeEntitize(cells[0].Trim()));
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        LogRenoDXModNameUnavailable(line);
                        skippedCount++;
                        continue;
                    }
                    
                    var (status, statusNote) = ParseStatusCell(cells[1]);
                    var columnNotes = cells.Length >= 3 ? cells[2].Trim() : null;
                    if (string.IsNullOrWhiteSpace(columnNotes))
                        columnNotes = null;

                    var notes = CombineNotes(statusNote, columnNotes);

                    if (notes is not null && RegexHelper.Match32BitRegex.IsMatch(notes))
                        architecture = Architecture.x32;

                    genericWikiMods.Add(new RenoDXGenericModInfoDto(
                        Name: name,
                        Status: status,
                        Notes: notes,
                        Architecture: architecture,
                        RenoDxWikiModType: currentEngine.Value
                    ));
                }
                else
                {
                    if (cells.Length < 4)
                    {
                        LogRenoDXSkipMalformedWikiModRow("at least 4", cells.Length, line);
                        skippedCount++;
                        continue;
                    }

                    var name = ExtractMarkdownLinkText(HtmlEntity.DeEntitize(cells[0].Trim()));
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        LogRenoDXModNameUnavailable(line);
                        skippedCount++;
                        continue;
                    }

                    var maintainer = cells[1].Trim();
                    if (string.IsNullOrWhiteSpace(maintainer))
                        maintainer = "Unknown";
                    var linksCell = cells[2].Trim();
                    var (status, statusNote) = ParseStatusCell(cells[3]);

                    wikiMods.Add(new RenoDXModInfoDto(
                        name,
                        ExtractMarkdownUrl(linksCell, "discord.com"),
                        ExtractMarkdownUrl(linksCell, ".addon64"),
                        ExtractMarkdownUrl(linksCell, ".addon32"),
                        ExtractMarkdownUrl(linksCell, "nexusmods.com"),
                        maintainer,
                        statusNote,
                        status
                    ));
                }
            }
        }
        catch (HttpRequestException ex)
        {
            LogSiteUnreachable(s_renoDxUrl, ex.StatusCode, ex);
        }
        catch (TaskCanceledException ex)
        {
            LogSiteTimeout(s_renoDxUrl, ex);
        }

        var dedupedUnrealGenericMods = DedupedUnrealMods(genericWikiMods);

        var engineNotesResult = engineNotes.ToImmutableDictionary(
            kv => kv.Key,
            kv => string.Join("\n", kv.Value).Trim());
        
        LogRenoDXModsFetchFinished(wikiMods.Count, genericWikiMods.Count, skippedCount);

        return new RenoDXWikiParseResultDto([.. wikiMods], [.. dedupedUnrealGenericMods],
            engineNotesResult);
    }

    private static string ExtractMarkdownLinkText(string text)
    {
        var bracketEnd = text.IndexOf("](", StringComparison.Ordinal);
        if (bracketEnd < 0) return text;

        var bracketStart = text.LastIndexOf('[', bracketEnd);
        if (bracketStart < 0) return text;

        return text[(bracketStart + 1)..bracketEnd].Trim();
    }

    private static string? ExtractMarkdownUrl(string markdown, string urlContains)
    {
        var start = 0;

        while (true)
        {
            var urlEnd = markdown.IndexOf(')', start);
            if (urlEnd < 0) return null;

            var urlStart = markdown.LastIndexOf('(', urlEnd);
            if (urlStart < 0) return null;

            var url = markdown[(urlStart + 1)..urlEnd];

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

            if (!match.Success) return null;

            LogReShadeSiteVersionFetchSuccess(s_reShadeSiteUrl);

            return match.Groups[1].Value;
        }
        catch (HttpRequestException ex)
        {
            LogSiteUnreachable(s_reShadeSiteUrl, ex.StatusCode, ex);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            LogSiteTimeout(s_reShadeSiteUrl, ex);
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
                LogReShadeTagsNotFound(s_reShadeTagsUrl);
                return versions;
            }

            foreach (var node in tagNodes)
            {
                var href = node.GetAttributeValue("href", string.Empty);
                var tag = href.Split('/').LastOrDefault();

                if (string.IsNullOrWhiteSpace(tag)) continue;

                var version = tag.TrimStart('v');

                if (string.IsNullOrWhiteSpace(version) || versions.Contains(version)) continue;

                versions.Add(version);
                LogReShadeVersionFound(version, s_reShadeTagsUrl);
            }
        }
        catch (HttpRequestException ex)
        {
            LogSiteUnreachable(s_reShadeTagsUrl, ex.StatusCode, ex);
            return versions;
        }
        catch (TaskCanceledException ex)
        {
            LogSiteTimeout(s_reShadeTagsUrl, ex);
            return versions;
        }

        return versions;
    }

    private async Task<ImmutableArray<string>> FetchRenoDXNightlyTagNamesAsync()
    {
        List<string> tags = [];

        try
        {
            var document = await LoadHtmlDocumentAsync(s_renoDXTagsUrl);

            var tagNodes = document.DocumentNode
                .SelectNodes("//a[contains(@href, 'clshortfuse/renodx/releases/tag/nightly-')]");

            if (tagNodes is null) return [];

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
            LogSiteUnreachable(s_renoDXTagsUrl, ex.StatusCode, ex);
            return [];
        }
        catch (TaskCanceledException ex)
        {
            LogSiteTimeout(s_renoDXTagsUrl, ex);
            return [];
        }

        return [.. tags];
    }

    private async Task<RenoDXTagInfoDto?> FetchRenoDXNightlyReleaseInfoAsync(string nightlyTag)
    {
        LogRenoDXNightlyTagParsingStart(nightlyTag);

        var renoDXNightlyTagUrl = s_renoDXReleasesTagUrl + nightlyTag;

        try
        {
            var dateStr = nightlyTag["nightly-".Length..];
            if (!DateOnly.TryParseExact(dateStr, "yyyyMMdd", null,
                    DateTimeStyles.None, out var date))
            {
                LogRenoDXNightlyTagDateParseFailure(nightlyTag, dateStr);
                return null;
            }

            var document = await LoadHtmlDocumentAsync(renoDXNightlyTagUrl);

            var preNode = document.DocumentNode
                .SelectSingleNode("//pre[contains(@class, 'text-small') and contains(@class, 'ws-pre-wrap')]");

            List<string> commitNotes = [];

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

            if (_logger.IsEnabled(LogLevel.Debug))
                LogRenoDXNightlyTagParsed(nightlyTag, string.Join(Environment.NewLine, commitNotes));

            return new RenoDXTagInfoDto(date, RenoDX.Branch.Nightly, commitNotes);
        }
        catch (HttpRequestException ex)
        {
            LogSiteUnreachable(renoDXNightlyTagUrl, ex.StatusCode, ex);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            LogSiteTimeout(renoDXNightlyTagUrl, ex);
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

    private static List<RenoDXGenericModInfoDto> DedupedUnrealMods(List<RenoDXGenericModInfoDto> mods)
    {
        var unrealVariants = mods.Where(m =>
            m.RenoDxWikiModType is RenoDXWikiModType.Unreal or RenoDXWikiModType.UnrealExtended);
        var others = mods.Where(m =>
            m.RenoDxWikiModType is not RenoDXWikiModType.Unreal and not RenoDXWikiModType.UnrealExtended);

        var dedupedUnreal = unrealVariants
            .GroupBy(m => GameNameHelper.NormalizeName(m.Name))
            .Select(g => g.FirstOrDefault(m =>
                m.RenoDxWikiModType == RenoDXWikiModType.UnrealExtended) ?? g.First());

        return [.. dedupedUnreal, .. others];
    }

    private static string CleanNotesLine(string line)
    {
        var match = GitHubAlertRegex().Match(line);

        if (match.Success)
            return $"{Capitalize(match.Groups["kind"].Value)}:";

        var withoutQuote = line.TrimStart('>').Trim();

        return StripMarkdownDecoration(withoutQuote);

        static string Capitalize(string s) => s.Length == 0
            ? s
            : char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant();
    }

    private static (string Status, string? Notes) ParseStatusCell(string rawStatus)
    {
        var trimmed = rawStatus.Trim();
        var match = StatusHoverRegex().Match(trimmed);

        if (!match.Success)
            return (trimmed, null);

        var title = StripMarkdownDecoration(HtmlEntity.DeEntitize(match.Groups["title"].Value.Trim()));

        return (match.Groups["icon"].Value, string.IsNullOrWhiteSpace(title) ? null : title);
    }

    private static string? CombineNotes(string? statusNote, string? columnNotes)
    {
        var cleanedColumNotes = columnNotes is null ? null : StripMarkdownDecoration(columnNotes);

        return (statusNote, cleanedColumNotes) switch
        {
            (null, null) => null,
            (var s, null) => s,
            (null, var c) => c,
            (var s, var c) => $"{s}\n\n{c}",
        };
    }

    private static string StripMarkdownDecoration(string text)
    {
        var withoutImages = MarkdownImageRegex().Replace(text, string.Empty);
        var withoutLinks = MarkdownLinkRegex().Replace(withoutImages, "$1");
        var withoutEmphasis = StripMarkdownEmphasis(withoutLinks);
        var withEmoji = EmojiShortcodeHelper.Convert(withoutEmphasis);

        return ExtraWhitespaceRegex().Replace(withEmoji, " ");
    }

    private static void AddNoteLine(Dictionary<RenoDXWikiModType, List<string>> engineNotes,
        RenoDXWikiModType renoDxWikiModType, string line)
    {
        if (!engineNotes.TryGetValue(renoDxWikiModType, out var noteLines))
        {
            noteLines = [];
            engineNotes[renoDxWikiModType] = noteLines;
        }

        noteLines.Add(line);
    }

    private static string StripMarkdownEmphasis(string text) => BoldRegex().Replace(text, "$1");
}