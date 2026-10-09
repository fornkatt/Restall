// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using Restall.Application.DTOs.RenoDXDTOs;

namespace Restall.Application.Services;

// RenoDX Game Entry Assembler — EventId range: 2100 - 2149
public partial class GameEntryAssembler
{
    [LoggerMessage(EventId = 2100, Level = LogLevel.Debug,
        Message = "Matched \"{GameName}\" to RenoDX mod \"{ModName}\" ({MatchKind})")]
    private partial void LogRenoDXModMatchFound(string gameName, string? modName, RenoDXModMatch.MatchKind matchKind);

    [LoggerMessage(EventId = 2101, Level = LogLevel.Debug,
        Message = "Could not find a RenoDX mod for \"{GameName}\"")]
    private partial void LogRenoDXModMatchNotFound(string gameName);

    [LoggerMessage(EventId = 2102, Level = LogLevel.Warning,
        Message = "Could not pick a RenoDX mod for \"{GameName}\" — tied entries: {TiedNames}")]
    private partial void LogRenoDXModMatchTie(string gameName, string tiedNames);
}
