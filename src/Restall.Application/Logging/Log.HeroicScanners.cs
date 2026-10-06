// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Domain.Entities;

namespace Restall.Application.Logging;

// Heroic Scanners Logging — EventId range: 75 - 99
public static partial class Log
{
    [LoggerMessage(EventId = 75, Level = LogLevel.Error,
        Message = "Failed to read the {Platform} Heroic library file \"{LibraryPath}\"")]
    public static partial void HeroicLibraryReadFailure(this ILogger logger, Game.Platform platform,
        string libraryPath, Exception ex);

    [LoggerMessage(EventId = 76, Level = LogLevel.Error,
        Message = "Failed to read the {Platform} Heroic 'installed.json' file in \"{InstalledJsonPath}\"")]
    public static partial void HeroicInstalledJsonReadFailure(this ILogger logger, Game.Platform platform,
        string installedJsonPath, Exception ex);

    [LoggerMessage(EventId = 77, Level = LogLevel.Error,
        Message = "Failed to scan {Platform} Heroic entry \"{Entry}\"")]
    public static partial void HeroicEntryScanFailure(this ILogger logger, Game.Platform platform, string entry,
        Exception ex);

    [LoggerMessage(EventId = 78, Level = LogLevel.Warning,
        Message = "Could not find the app name for {Platform} Heroic game at \"{InstallPath}\"")]
    public static partial void HeroicAppNameNotFound(this ILogger logger, Game.Platform platform, string installPath);

    [LoggerMessage(EventId = 79, Level = LogLevel.Warning,
        Message = "Could not find the install path for {Platform} Heroic game with app name: \"{AppName}\"")]
    public static partial void HeroicInstallPathNotFound(this ILogger logger, Game.Platform platform,
        string? appName);

    [LoggerMessage(EventId = 80, Level = LogLevel.Warning,
        Message =
            "Could not find the name for {Platform} Heroic game with \"{AppName}\" at: \"{InstallPath}\"")]
    public static partial void HeroicGameNameNotFound(this ILogger logger, Game.Platform platform, string appName,
        string installPath);

    [LoggerMessage(EventId = 81, Level = LogLevel.Warning,
        Message = "No entries found in {Platform} Heroic library file \"{LibraryPath}\"" +
                  " — all {Platform} Heroic games will be skipped")]
    public static partial void HeroicLibraryEmpty(this ILogger logger, Game.Platform platform,
        string libraryPath);

    [LoggerMessage(EventId = 82, Level = LogLevel.Warning,
        Message =
            "Could not find library entry for {Platform} Heroic game \"{AppName}\" in \"{LibraryPath}\"")]
    public static partial void HeroicLibraryEntryNotFound(this ILogger logger, Game.Platform platform,
        string? appName, string libraryPath);

    [LoggerMessage(EventId = 83, Level = LogLevel.Debug,
        Message = "No entries found in {Platform} Heroic installed file \"{InstalledJsonPath}\"")]
    public static partial void HeroicInstalledEmpty(this ILogger logger, Game.Platform platform,
        string installedJsonPath);
}
