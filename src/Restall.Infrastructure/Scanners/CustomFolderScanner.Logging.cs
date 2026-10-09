// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace Restall.Infrastructure.Scanners;

// Custom Folder Scanner Logging — EventId range: 2050 - 2099
internal sealed partial class CustomFolderScanner
{
    [LoggerMessage(EventId = 2050, Level = LogLevel.Warning,
        Message = "Could not find custom game folder \"{FolderPath}\"")]
    private partial void LogCustomGameFolderNotFound(string folderPath);
}
