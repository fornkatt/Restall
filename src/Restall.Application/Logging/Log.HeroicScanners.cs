// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Domain.Entities;

namespace Restall.Application.Logging;

// Heroic Scanners Logging — EventId range: 75 - 99
public static partial class Log
{
    [LoggerMessage(EventId = 75, Level = LogLevel.Error,
        Message = "Failed to read the {Platform} Heroic install info file \"{InstallInfoPath}\"")]
    public static partial void HeroicInstallInfoReadFailure(this ILogger logger, Game.Platform platform,
        string installInfoPath, Exception ex);

    [LoggerMessage(EventId = 76, Level = LogLevel.Error,
        Message = "Failed to read the {Platform} Heroic 'installed.json' file in \"{InstalledJsonPath}\"")]
    public static partial void HeroicInstalledJsonReadFailure(this ILogger logger, Game.Platform platform,
        string installedJsonPath, Exception ex);

    [LoggerMessage(EventId = 77, Level = LogLevel.Error,
        Message = "Failed to scan {Platform} Heroic JSON block \"{Json}\"")]
    public static partial void HeroicJsonBlockScanFailure(this ILogger logger, Game.Platform platform, string json,
        Exception ex);

    [LoggerMessage(EventId = 78, Level = LogLevel.Warning,
        Message = "Could not find the app name for {Platform} Heroic game at \"{InstallPath}\"")]
    public static partial void HeroicAppNameNotFound(this ILogger logger, Game.Platform platform, string? installPath);

    [LoggerMessage(EventId = 79, Level = LogLevel.Warning,
        Message = "Could not find the install path for {Platform} Heroic game with app name: \"{AppName}\"")]
    public static partial void HeroicInstallPathNotFound(this ILogger logger, Game.Platform platform,
        string? appName);

    [LoggerMessage(EventId = 80, Level = LogLevel.Warning,
        Message =
            "Could not find the name for {Platform} Heroic game with \"{AppName}\" at: \"{InstallPath}\"")]
    public static partial void HeroicGameNameNotFound(this ILogger logger, Game.Platform platform, string? appName,
        string installPath);

    [LoggerMessage(EventId = 81, Level = LogLevel.Warning,
        Message = "No entries found in {Platform} Heroic install info file \"{InstallInfoPath}\"" +
                  " — all {Platform} Heroic games will be skipped")]
    public static partial void HeroicInstallInfoEmpty(this ILogger logger, Game.Platform platform,
        string installInfoPath);

    [LoggerMessage(EventId = 82, Level = LogLevel.Warning,
        Message =
            "Could not find install info entry for {Platform} Heroic game \"{AppName}\" in \"{InstallInfoPath}\"")]
    public static partial void HeroicInstallInfoEntryNotFound(this ILogger logger, Game.Platform platform,
        string? appName, string installInfoPath);
}
