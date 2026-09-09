// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Scanners;

// Epic Scanner Logging — EventId range: 1650 - 1699
internal sealed partial class EpicScanner
{
    [LoggerMessage(EventId = 1650, Level = LogLevel.Error,
        Message = "Failed to scan items in Epic Games manifest \"{File}\"")]
    private partial void LogEpicManifestScanFailure(string file, Exception ex);

    [LoggerMessage(EventId = 1651, Level = LogLevel.Warning,
        Message = "Could not find the name of the Epic game \"{File}\" in \"{Item}\" in Epic Games Manifest")]
    private partial void LogEpicGameNameNotFound(string file, string item);

    [LoggerMessage(EventId = 1652, Level = LogLevel.Warning,
        Message = "Could not find root path for Epic game \"{Name}\" with item: \"{Item}\"")]
    private partial void LogEpicGameRootPathNotFound(string name, string item);
}