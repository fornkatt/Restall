// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.DTOs.Responses;
using Restall.Application.Helpers;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Services;

internal sealed partial class ParseService : IParseService
{
    internal const string HttpClientName = nameof(ParseService);

    private const string ReShadeTagsUrl = "https://github.com/crosire/reshade/tags";
    private const string ReShadeSiteUrl = "https://reshade.me";

    private const string RenoDXUrl = "https://raw.githubusercontent.com/wiki/clshortfuse/renodx/Mods.md";
    private const string RenoDXTagsUrl = "https://github.com/clshortfuse/renodx/tags";

    private const string
        RenoDXTagReleasesUrl =
            "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd

    private const string RenoDXExpandedAssetsUrl = "https://github.com/clshortfuse/renodx/releases/expanded_assets/";
    private const string RenoDXDownloadUrl = "https://github.com/clshortfuse/renodx/releases/download/";

    private readonly IHttpClientFactory _clientFactory;
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
        IHttpClientFactory clientFactory
    )
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    // TODO: need better catch safety, global exception handler?
    public async Task<ImmutableArray<string>> FetchReShadeVersionsAsync()
    {
        LogReShadeVersionFetchStart(ReShadeSiteUrl, ReShadeTagsUrl);

        var versions = await FetchReShadeVersionsFromGitHubTagsAsync();
        var siteVersion = await FetchLatestReShadeVersionFromSiteAsync();

        if (siteVersion is not null && !versions.Contains(siteVersion))
        {
            versions.Insert(0, siteVersion);
            LogReShadeSiteVersionNewer(ReShadeSiteUrl, ReShadeTagsUrl, siteVersion);
        }

        if (_logger.IsEnabled(LogLevel.Information))
            LogReShadeVersionFetchComplete(versions.Count, versions.FirstOrDefault());

        return [.. versions];
    }

    public async Task<Result<RenoDXTagInfo>> FetchRenoDXSnapshotAsync(CancellationToken cancellationToken = default)
    {
        const string tag = "snapshot";
        const string snapshotUrl = RenoDXTagReleasesUrl + tag;

        HtmlDocument document;

        LogRenoDXSnapshotFetchStart(snapshotUrl);

        try
        {
            document = await LoadHtmlDocumentAsync(snapshotUrl, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<RenoDXTagInfo>.Error($"Could not load the RenoDX snapshot page \"{snapshotUrl}\"",
                ErrorType.PageLoadFailure, ex);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<RenoDXTagInfo>.Error($"Request to \"{snapshotUrl}\" timed out", ErrorType.NetworkTimeout, ex);
        }

        var datetime = document.DocumentNode.SelectSingleNode("//relative-time")
            ?.GetAttributeValue("datetime", string.Empty) ?? string.Empty;

        if (!DateTime.TryParse(datetime, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal,
                out var releaseTime))
            return Result<RenoDXTagInfo>.Error($"Could not find the release date on \"{snapshotUrl}\" " +
                                               $"— value on the page: \"{datetime}\"");

        var date = DateOnly.FromDateTime(releaseTime);
        var commitNotes = FetchRenoDXSnapshotCommitNotes(document);
        var downloadBaseUrl = new Uri(RenoDXDownloadUrl + tag + "/");
        var addonFilenames = await FetchRenoDXAddonFilenamesAsync(tag, cancellationToken);

        if (addonFilenames.Count == 0)
            return Result<RenoDXTagInfo>.Partial(new RenoDXTagInfo(date, RenoDX.Branch.Snapshot, downloadBaseUrl,
                    [], commitNotes),
                "Could not fetch addon filenames for latest 'RenoDX' snapshot",
                WarningType.RenoDXSnapshotFileListUnavailable);

        if (_logger.IsEnabled(LogLevel.Debug))
            LogRenoDXSnapshotCommitNotesFetchComplete(string.Join(Environment.NewLine, commitNotes));

        LogRenoDXSnapshotFetchSuccess(date, addonFilenames.Count);

        return Result<RenoDXTagInfo>.Success(new RenoDXTagInfo(date, RenoDX.Branch.Snapshot, downloadBaseUrl,
            addonFilenames, commitNotes));
    }

    public async Task<Result<ImmutableArray<RenoDXTagInfo>>> FetchRenoDXNightlyTagsAsync(
        CancellationToken cancellationToken = default)
    {
        HtmlDocument document;

        LogRenoDXNightliesFetchStart(RenoDXTagsUrl);

        try
        {
            document = await LoadHtmlDocumentAsync(RenoDXTagsUrl, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result<ImmutableArray<RenoDXTagInfo>>.Error(
                $"Could not load the 'RenoDX' tags page \"{RenoDXTagsUrl}\"",
                ErrorType.PageLoadFailure, ex);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<ImmutableArray<RenoDXTagInfo>>.Error($"Request to \"{RenoDXTagsUrl}\" timed out",
                ErrorType.NetworkTimeout, ex);
        }

        var nightlyTags = FetchRenoDXNightlyTagNames(document);

        if (nightlyTags.IsEmpty)
            return Result<ImmutableArray<RenoDXTagInfo>>.Error($"Could not find any 'RenoDX' on \"{RenoDXTagsUrl}\"");

        var nightlyResults = await Task.WhenAll(
            nightlyTags.Select(nightlyTag => FetchRenoDXNightlyAsync(nightlyTag, cancellationToken)));
        var nightlies = nightlyResults.OfType<RenoDXTagInfo>().ToImmutableArray();

        LogRenoDXNightliesFetchComplete(nightlies.Length, nightlyTags.Length,
            nightlies.FirstOrDefault()?.Version);

        if (nightlies.IsEmpty)
            return Result<ImmutableArray<RenoDXTagInfo>>
                .Error($"Could not load any of the {nightlyTags.Length} RenoDX nightly releases");

        if (nightlies.Length < nightlyTags.Length)
            return Result<ImmutableArray<RenoDXTagInfo>>.Partial(nightlies,
                $"Could not load {nightlyTags.Length - nightlies.Length} RenoDX nightly tags",
                WarningType.RenoDXNightliesIncomplete);

        return Result<ImmutableArray<RenoDXTagInfo>>.Success(nightlies);
    }

    private async Task<string?> FetchLatestReShadeVersionFromSiteAsync()
    {
        try
        {
            var httpClient = _clientFactory.CreateClient(HttpClientName);
            var document = await httpClient.GetStringAsync(ReShadeSiteUrl);
            var match = RegexHelper.ExtractReShadeVersionFromSite.Match(document);

            if (!match.Success) return null;

            LogReShadeSiteVersionFetchSuccess(ReShadeSiteUrl);

            return match.Groups[1].Value;
        }
        catch (HttpRequestException ex)
        {
            LogSiteUnreachable(ReShadeSiteUrl, ex.StatusCode, ex);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            LogSiteTimeout(ReShadeSiteUrl, ex);
            return null;
        }
    }

    private async Task<List<string>> FetchReShadeVersionsFromGitHubTagsAsync()
    {
        var versions = new List<string>();

        try
        {
            var document = await LoadHtmlDocumentAsync(ReShadeTagsUrl);

            var tagNodes = document.DocumentNode
                .SelectNodes("//a[contains(@href, 'crosire/reshade/releases/tag/')]");

            if (tagNodes is null)
            {
                LogReShadeTagsNotFound(ReShadeTagsUrl);
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
                LogReShadeVersionFound(version, ReShadeTagsUrl);
            }
        }
        catch (HttpRequestException ex)
        {
            LogSiteUnreachable(ReShadeTagsUrl, ex.StatusCode, ex);
            return versions;
        }
        catch (TaskCanceledException ex)
        {
            LogSiteTimeout(ReShadeTagsUrl, ex);
            return versions;
        }

        return versions;
    }

    private async Task<RenoDXTagInfo?> FetchRenoDXNightlyAsync(string nightlyTag, CancellationToken cancellationToken)
    {
        LogRenoDXNightlyTagParsingStart(nightlyTag);

        var dateText = nightlyTag["nightly-".Length..];

        if (!DateOnly.TryParseExact(dateText, RenoDX.VersionFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
        {
            LogRenoDXNightlyTagDateParseFailure(nightlyTag, dateText);
            return null;
        }

        var nightlyUrl = RenoDXTagReleasesUrl + nightlyTag;
        HtmlDocument document;

        try
        {
            document = await LoadHtmlDocumentAsync(nightlyUrl, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            LogRenoDXNightlyTagFetchFailure(nightlyTag, nightlyUrl, ex);
            return null;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            LogRenoDXNightlyTagFetchFailure(nightlyTag, nightlyUrl, ex);
            return null;
        }

        var addonFilenames = await FetchRenoDXAddonFilenamesAsync(nightlyTag, cancellationToken);

        if (addonFilenames.Count == 0)
            return null;

        var commitNotes = FetchRenoDXNightlyCommitNotes(document);

        if (_logger.IsEnabled(LogLevel.Debug))
            LogRenoDXNightlyTagParseComplete(nightlyTag, string.Join(Environment.NewLine, commitNotes));

        return new RenoDXTagInfo(date, RenoDX.Branch.Nightly, new Uri(RenoDXDownloadUrl + nightlyTag + "/"),
            addonFilenames, commitNotes);
    }

    private async Task<FrozenSet<string>> FetchRenoDXAddonFilenamesAsync(string tag,
        CancellationToken cancellationToken = default)
    {
        var assetUrl = RenoDXExpandedAssetsUrl + tag;
        HtmlDocument document;

        try
        {
            document = await LoadHtmlDocumentAsync(assetUrl, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            LogRenoDXAddonFileListFetchFailure(assetUrl, ex);
            return [];
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            LogRenoDXAddonFileListFetchFailure(assetUrl, ex);
            return [];
        }

        var downloadPath = $"/clshortfuse/renodx/releases/download/{tag}/";
        var linkNodes = document.DocumentNode
            .SelectNodes($"//a[starts-with(@href, '{downloadPath}')]");

        if (linkNodes is null)
        {
            LogRenoDXAddonFileListEmpty(assetUrl);
            return [];
        }

        List<string> addonFilenames = [];

        foreach (var linkNode in linkNodes)
        {
            var href = HtmlEntity.DeEntitize(linkNode.GetAttributeValue("href", string.Empty));
            var filename = Uri.UnescapeDataString(href[downloadPath.Length..]);

            if (filename.EndsWith(RenoDX.AddonExtension, StringComparison.OrdinalIgnoreCase) ||
                filename.EndsWith(RenoDX.AddonExtension32, StringComparison.OrdinalIgnoreCase))
                addonFilenames.Add(filename);
        }

        if (addonFilenames.Count == 0)
        {
            LogRenoDXAddonFileListEmpty(assetUrl);
            return [];
        }

        return addonFilenames.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    private static ImmutableArray<string> FetchRenoDXNightlyTagNames(HtmlDocument document)
    {
        var tagNodes = document.DocumentNode
            .SelectNodes("//a[contains(@href, 'clshortfuse/renodx/releases/tag/nightly-')]");

        if (tagNodes is null)
            return [];

        List<string> tags = [];

        foreach (var node in tagNodes)
        {
            var tag = node.GetAttributeValue("href", string.Empty).Split('/').LastOrDefault();

            if (string.IsNullOrWhiteSpace(tag) || !tag.StartsWith("nightly-") || tags.Contains(tag))
                continue;

            tags.Add(tag);
        }

        return [.. tags];
    }

    private static List<string> FetchRenoDXSnapshotCommitNotes(HtmlDocument document)
    {
        var bodyNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'markdown-body')]");
        List<string> commitNotes = [];

        if (bodyNode is null)
            return commitNotes;

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

        return commitNotes;
    }

    private static List<string> FetchRenoDXNightlyCommitNotes(HtmlDocument document)
    {
        var preNode = document.DocumentNode
            .SelectSingleNode("//pre[contains(@class, 'text-small') and contains(@class, 'ws-pre-wrap')]");

        if (preNode is null)
            return [];

        return HtmlEntity.DeEntitize(preNode.InnerText)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Skip(1)
            .ToList();
    }

    private async Task<HtmlDocument> LoadHtmlDocumentAsync(string url, CancellationToken cancellationToken = default)
    {
        var httpClient = _clientFactory.CreateClient(HttpClientName);
        var html = await httpClient.GetStringAsync(url, cancellationToken);
        var document = new HtmlDocument();
        document.LoadHtml(html);
        return document;
    }

    // TODO: surface Result<T>
    public async Task<RenoDXWikiParseResultDto> FetchRenoDXWikiModsAsync()
    {
        LogRenoDXWikiModsFetchStart(RenoDXUrl);

        var skippedCount = 0;

        var wikiMods = new List<RenoDXModInfoDto>();
        var genericWikiMods = new List<RenoDXGenericModInfoDto>();
        var engineNotes = new Dictionary<RenoDXWikiModType, List<string>>();

        try
        {
            var httpClient = _clientFactory.CreateClient(HttpClientName);
            var markdown = await httpClient.GetStringAsync(RenoDXUrl);
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

                if (line.StartsWith("### Unreal Engine", StringComparison.OrdinalIgnoreCase) &&
                    !line.Contains("Extended", StringComparison.OrdinalIgnoreCase))
                {
                    currentEngine = RenoDXWikiModType.Unreal;
                    capturingNotesFor = currentEngine;
                    inTable = false;
                    headerSkipped = false;
                    inCodeFence = false;
                    continue;
                }

                if (line.StartsWith("### Unreal Engine Extended", StringComparison.OrdinalIgnoreCase))
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
                    var architecture = Architecture.X64;

                    if (cells.Length < 2)
                    {
                        LogRenoDXMalformedWikiModRowSkipped("at least 2", cells.Length, line);
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

                    if (ArchitectureRecommender.Mentions32Bit(notes))
                        architecture = Architecture.X32;

                    genericWikiMods.Add(new RenoDXGenericModInfoDto(
                        Name: name,
                        Status: status,
                        Notes: notes,
                        Architecture: architecture,
                        RenoDXWikiModType: currentEngine.Value
                    ));
                }
                else
                {
                    if (cells.Length < 4)
                    {
                        LogRenoDXMalformedWikiModRowSkipped("at least 4", cells.Length, line);
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
            LogSiteUnreachable(RenoDXUrl, ex.StatusCode, ex);
        }
        catch (TaskCanceledException ex)
        {
            LogSiteTimeout(RenoDXUrl, ex);
        }

        var dedupedUnrealGenericMods = DedupedUnrealMods(genericWikiMods);

        var engineNotesResult = engineNotes.ToImmutableDictionary(
            kv => kv.Key,
            kv => string.Join("\n", kv.Value).Trim());

        LogRenoDXModsFetchComplete(wikiMods.Count, genericWikiMods.Count, skippedCount);

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

    private static List<RenoDXGenericModInfoDto> DedupedUnrealMods(List<RenoDXGenericModInfoDto> mods)
    {
        var unrealVariants = mods.Where(m =>
            m.RenoDXWikiModType is RenoDXWikiModType.Unreal or RenoDXWikiModType.UnrealExtended);
        var others = mods.Where(m =>
            m.RenoDXWikiModType is not RenoDXWikiModType.Unreal and not RenoDXWikiModType.UnrealExtended);

        var dedupedUnreal = unrealVariants
            .GroupBy(m => GameNameHelper.NormalizeName(m.Name))
            .Select(g => g.FirstOrDefault(m =>
                m.RenoDXWikiModType == RenoDXWikiModType.UnrealExtended) ?? g.First());

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
