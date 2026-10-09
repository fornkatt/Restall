// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace Restall.Application.UseCases;

// Library Refresh Logging — EventId range: 1250 - 1299
public sealed partial class FullLibraryRefreshUseCase
{
    [LoggerMessage(EventId = 1250, Level = LogLevel.Warning,
        Message = "Could not find any ReShade versions — the ReShade version list was not refreshed")]
    private partial void LogReShadeVersionsNotFound();

    [LoggerMessage(EventId = 1251, Level = LogLevel.Warning,
        Message = "Failed to fetch a RenoDX mod database file — Service returned: \"{ErrorMessage}\"")]
    private partial void LogRenoDXModDatabaseFileFetchFailure(string? errorMessage, Exception? ex);

    [LoggerMessage(EventId = 1252, Level = LogLevel.Warning,
        Message = "Could not build the RenoDX mod database — Service returned: \"{ErrorMessage}\"")]
    private partial void LogRenoDXModDatabaseBuildFailure(string? errorMessage);

    [LoggerMessage(EventId = 1253, Level = LogLevel.Warning,
        Message = "Failed to fetch the RenoDX Snapshot release — Service returned: \"{ErrorMessage}\"")]
    private partial void LogRenoDXSnapshotFetchFailure(string? errorMessage, Exception? ex);

    [LoggerMessage(EventId = 1254, Level = LogLevel.Warning,
        Message = "Failed to fetch the RenoDX Nightly releases — Service returned: \"{ErrorMessage}\"")]
    private partial void LogRenoDXNightliesFetchFailure(string? errorMessage, Exception? ex);
}
