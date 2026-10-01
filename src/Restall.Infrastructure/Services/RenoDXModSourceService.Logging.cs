// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Services;

// RenoDX Mod Source Fetch Logging — EventId range: 1950 - 1999
internal sealed partial class RenoDXModSourceService
{
    [LoggerMessage(EventId = 1950, Level = LogLevel.Information,
        Message = "Fetching RenoDX mod source file \"{Url}\"")]
    private partial void LogRenoDXDatabaseFileFetchStart(string url);

    [LoggerMessage(EventId = 1951, Level = LogLevel.Information,
        Message = "Read {EntryCount} RenoDX mod entries from \"{Url}\" — skipped count: {SkippedCount}")]
    private partial void LogRenoDXDatabaseFileReadComplete(int entryCount, string url, int skippedCount);

    [LoggerMessage(EventId = 1952, Level = LogLevel.Warning,
        Message = "Failed to read RenoDX mod entry in \"{Url}\"\n" +
                  "Raw entry: {RawEntry}")]
    private partial void LogRenoDXDatabaseFileEntryReadFailure(string url, string rawEntry,
        Exception ex);

    [LoggerMessage(EventId = 1953, Level = LogLevel.Warning,
        Message = "Could not find 'RenoDX' mod entry in \"{Url}\" — found 'null', entry skipped")]
    private partial void LogRenoDXDatabaseFileEntryNotFound(string url);
}
