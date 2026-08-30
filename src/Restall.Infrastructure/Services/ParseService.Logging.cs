using System.Net;
using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Services;

// Web Parse Logging — EventId range: 1100 - 1149
internal sealed partial class ParseService
{
    [LoggerMessage(EventId = 1100, Level = LogLevel.Information,
        Message = "Fetching latest ReShade versions from {ReShadeSite} and {ReShadeGitHubTags}")]
    private partial void LogReShadeVersionFetchStart(string reShadeSite, string reShadeGitHubTags);

    [LoggerMessage(EventId = 1101, Level = LogLevel.Debug,
        Message = "{ReShadeSite} has a newer version not yet on {GitHubTags}: {SiteVersion}")]
    private partial void LogReShadeSiteVersionNewer(string reShadeSite, string gitHubTags, string siteVersion);

    /// <summary>
    ///     Must be guarded at the call site with ILogger.IsEnabled(LogLevel.Information)
    /// </summary>
    [LoggerMessage(EventId = 1102, Level = LogLevel.Information, SkipEnabledCheck = true,
        Message = "Fetched {VersionCount} stable ReShade versions. Latest: {LatestVersion}")]
    private partial void LogReShadeVersionFetchFinished(int versionCount, string? latestVersion);

    [LoggerMessage(EventId = 1103, Level = LogLevel.Information,
        Message = "Fetching latest RenoDX snapshot information from {RenoDXSnapshotUrl}")]
    private partial void LogRenoDXSnapshotFetchStart(string renoDXSnapshotUrl);

    [LoggerMessage(EventId = 1104, Level = LogLevel.Warning,
        Message =
            "Failed to parse snapshot release date. Snapshot will be unavailable. Value from site: {OriginalValue}")]
    private partial void LogRenoDXSnapshotReleaseDateParseFailure(string originalValue);

    [LoggerMessage(EventId = 1105, Level = LogLevel.Information,
        Message = "Successfully fetched RenoDX snapshot: {SnapshotVersion}")]
    private partial void LogRenoDXSnapshotFetchSuccess(DateOnly snapshotVersion);

    /// <summary>
    ///     Must be guarded at the call site with ILogger.IsEnabled(LogLevel.Debug)
    /// </summary>
    [LoggerMessage(EventId = 1106, Level = LogLevel.Debug, SkipEnabledCheck = true,
        Message = "Snapshot commit notes:\n{Notes}")]
    private partial void LogRenoDXSnapshotFetchCommitNotes(string notes);

    [LoggerMessage(EventId = 1107, Level = LogLevel.Error,
        Message = "{Site} is unreachable ({StatusCode})")]
    private partial void LogSiteUnreachable(string site, HttpStatusCode? statusCode,
        Exception ex);

    [LoggerMessage(EventId = 1108, Level = LogLevel.Error,
        Message = "Request for {Site} timed out")]
    private partial void LogSiteTimeout(string site, Exception ex);

    [LoggerMessage(EventId = 1109, Level = LogLevel.Information,
        Message = "Fetching latest RenoDX nightly versions from {RenoDXTagsUrl}")]
    private partial void LogFetchingRenoDXNightlyVersions(string renoDXTagsUrl);

    [LoggerMessage(EventId = 1110, Level = LogLevel.Warning,
        Message = "No RenoDX nightly versions found. Nightly versions will be unavailable.")]
    private partial void LogRenoDXNightlyVersionsNotFound();

    [LoggerMessage(EventId = 1111, Level = LogLevel.Information,
        Message = "Fetched {NightlyCount} nightly RenoDX versions. Latest: {LatestVersion}")]
    private partial void LogRenoDXNightlyVersionsFetched(int nightlyCount, string? latestVersion);

    [LoggerMessage(EventId = 1112, Level = LogLevel.Information,
        Message = "Fetching available RenoDX mods from main wiki page: {Url}")]
    private partial void LogFetchingRenoDXModsFromWiki(string url);

    [LoggerMessage(EventId = 1113, Level = LogLevel.Warning,
        Message = "Skipping malformed RenoDX wiki mod row. " +
                  "Expected cell count: {ExpectedCount} || Was: {ActualCount}\n{Line}")]
    private partial void LogRenoDXSkipMalformedWikiModRow(int expectedCount, int actualCount, string line);

    [LoggerMessage(EventId = 1114, Level = LogLevel.Warning,
        Message = "Skipping malformed RenoDX row. No mod name available.\n{Line}")]
    private partial void LogRenoDXModNameUnavailable(string line);

    [LoggerMessage(EventId = 1115, Level = LogLevel.Information,
        Message = "Successfully fetched {RenoDXModCount} RenoDX mods and {RenoDXGenericModCount} generic RenoDX mods." +
                  " Skipped due to malformed rows: {SkippedCount}")]
    private partial void LogRenoDXModsFetchFinished(int renoDXModCount, int renoDXGenericModCount, int skippedCount);

    [LoggerMessage(EventId = 1116, Level = LogLevel.Information,
        Message = "Successfully fetched latest ReShade version from {ReShadeSiteUrl}")]
    private partial void LogReShadeSiteVersionFetchSuccess(string reShadeSiteUrl);

    [LoggerMessage(EventId = 1117, Level = LogLevel.Warning,
        Message = "No ReShade tags found on {ReShadeGitHubTagsUrl}")]
    private partial void LogReShadeTagsNotFound(string reShadeGitHubTagsUrl);

    [LoggerMessage(EventId = 1118, Level = LogLevel.Debug,
        Message = "Found ReShade version {Version} on {ReShadeGitHubTagsUrl}")]
    private partial void LogReShadeVersionFound(string version, string reShadeGitHubTagsUrl);

    [LoggerMessage(EventId = 1119, Level = LogLevel.Debug,
        Message = "Parsing RenoDX nightly tag {NightlyTag} information")]
    private partial void LogRenoDXNightlyTagParsingStart(string nightlyTag);

    [LoggerMessage(EventId = 1120, Level = LogLevel.Warning,
        Message = "Could not parse date from nightly tag {NightlyTag} — Original value from site: {OriginalValue}")]
    private partial void LogRenoDXNightlyTagDateParseFailure(string nightlyTag, string originalValue);

    /// <summary>
    ///     Must be guarded at the call site with ILogger.IsEnabled(LogLevel.Debug)
    /// </summary>
    [LoggerMessage(EventId = 1121, Level = LogLevel.Debug, SkipEnabledCheck = true,
        Message = "Successfully parsed RenoDX nightly tag {NightlyTag}\n" +
                  "Commit notes:\n" +
                  "{CommitNotes}")]
    private partial void LogRenoDXNightlyTagParsed(string nightlyTag, string commitNotes);
}