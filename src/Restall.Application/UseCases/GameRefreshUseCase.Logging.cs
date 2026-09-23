// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace Restall.Application.UseCases;

// Game Refresh Logging — EventId range: 1900 - 1949
public sealed partial class GameRefreshUseCase
{
    [LoggerMessage(EventId = 1250, Level = LogLevel.Debug,
        Message = "Found compatible RenoDX mod for \"{GameName}\" — Mod name: \"{ModName}\"")]
    private partial void LogRenoDXCompatibleGameFound(string gameName, string modName);

    [LoggerMessage(EventId = 1251, Level = LogLevel.Debug,
        Message = "Found compatible generic RenoDX mod for \"{GameName}\" — Mod name: \"{ModName}\"")]
    private partial void LogRenoDXCompatibleGenericGameFound(string gameName,
        string modName);

    [LoggerMessage(EventId = 1252, Level = LogLevel.Debug,
        Message = "No compatible RenoDX game found for \"{GameName}\"")]
    private partial void LogRenoDXCompatibleGameNotFound(string gameName);
}
