// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Scanners;

// Xbox Scanner Logging — EventId range: 1850 - 1899
internal sealed partial class XboxScanner
{
    [LoggerMessage(EventId = 1850, Level = LogLevel.Error,
        Message = "Failed to scan Xbox library \"{GameDirectory}\"")]
    private partial void LogXboxScanFailure(string gameDirectory, Exception ex);

    [LoggerMessage(EventId = 1851, Level = LogLevel.Warning,
        Message = "Could not find game name in \"{GameDirectory}\"")]
    private partial void LogXboxGameNameNotFound(string gameDirectory);

    [LoggerMessage(EventId = 1852, Level = LogLevel.Warning,
        Message = "Could not find the content directory \"{SubDir}\" for \"{GameDirectory}\"")]
    private partial void LogXboxContentDirNotFound(string subDir, string gameDirectory);

    [LoggerMessage(EventId = 1853, Level = LogLevel.Warning,
        Message = "Could not find the 'Microsoft Game Config' for Xbox game \"{SubDir}\" in \"{ConfigPath}\"")]
    private partial void LogXboxGameConfigNotFound(string subDir, string configPath);
}