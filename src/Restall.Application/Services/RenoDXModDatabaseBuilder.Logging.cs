// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;

namespace Restall.Application.Services;

// RenoDX Database Builder — EventId range: 2000 - 2049
public sealed partial class RenoDXModDatabaseBuilder
{
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information,
        Message = "Building RenoDX mod database")]
    private partial void LogRenoDXDatabaseBuildStart();

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information,
        Message = "Finished building RenoDX mod database. Game mods {GameModCount}," +
                  " Unreal generic mods: {UnrealGenericModCount}, Unity generic mods: {UnityGenericModCount}")]
    private partial void LogRenoDXDatabaseBuildComplete(int gameModCount, int unrealGenericModCount,
        int unityGenericModCount);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning,
        Message = "Could not use RenoDX mod entry due to: {DropReason} — entry dropped: \"{Entry}\"")]
    private partial void LogRenoDXDatabaseEntryValidationFailure(string dropReason, string entry);
}
