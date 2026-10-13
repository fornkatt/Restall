// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Helpers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;

namespace Restall.Infrastructure.Services;

internal sealed partial class ParseService : IParseService
{
    internal const string HttpClientName = nameof(ParseService);

    private const string ReShadeTagsUrl = "https://github.com/crosire/reshade/tags";
    private const string ReShadeSiteUrl = "https://reshade.me";

    private const string RenoDXTagsUrl = "https://github.com/clshortfuse/renodx/tags";

    private const string
        RenoDXTagReleasesUrl =
            "https://github.com/clshortfuse/renodx/releases/tag/"; // Follow by snapshot or nightly-yyyyMMdd

    private const string RenoDXExpandedAssetsUrl = "https://github.com/clshortfuse/renodx/releases/expanded_assets/";
    private const string RenoDXDownloadUrl = "https://github.com/clshortfuse/renodx/releases/download/";

    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<ParseService> _logger;

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
            return Result<RenoDXTagInfo>.Partial(new RenoDXTagInfo(date, downloadBaseUrl,
                    [], [.. commitNotes]),
                "Could not fetch addon filenames for latest 'RenoDX' snapshot",
                WarningType.RenoDXSnapshotFileListUnavailable);

        if (_logger.IsEnabled(LogLevel.Debug))
            LogRenoDXSnapshotCommitNotesFetchComplete(string.Join(Environment.NewLine, commitNotes));

        LogRenoDXSnapshotFetchSuccess(date, addonFilenames.Count);

        return Result<RenoDXTagInfo>.Success(new RenoDXTagInfo(date, downloadBaseUrl,
            addonFilenames, [.. commitNotes]));
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
            return Result<ImmutableArray<RenoDXTagInfo>>.Error(
                $"Could not find any 'RenoDX' Nightly tags on \"{RenoDXTagsUrl}\"");

        var nightlyResults = await Task.WhenAll(
            nightlyTags.Select(nightlyTag => FetchRenoDXNightlyAsync(nightlyTag, cancellationToken)));
        var nightlies = nightlyResults.OfType<RenoDXTagInfo>().ToImmutableArray();

        LogRenoDXNightliesFetchComplete(nightlies.Length, nightlyTags.Length,
            nightlies.MaxBy(n => n.Date)?.Version);

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

        var dateText = nightlyTag[RenoDX.NightlyTagPrefix.Length..];

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

        return new RenoDXTagInfo(date, new Uri(RenoDXDownloadUrl + nightlyTag + "/"),
            addonFilenames, [.. commitNotes]);
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
            .SelectNodes($"//a[contains(@href, 'clshortfuse/renodx/releases/tag/{RenoDX.NightlyTagPrefix}')]");

        if (tagNodes is null)
            return [];

        List<string> tags = [];

        foreach (var node in tagNodes)
        {
            var tag = node.GetAttributeValue("href", string.Empty).Split('/').LastOrDefault();

            if (string.IsNullOrWhiteSpace(tag)
                || !tag.StartsWith(RenoDX.NightlyTagPrefix, StringComparison.Ordinal)
                || tags.Contains(tag))
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
}
