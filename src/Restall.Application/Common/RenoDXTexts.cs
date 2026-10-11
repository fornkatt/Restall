// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using System.Diagnostics;

namespace Restall.Application.Common;

public static class RenoDXTexts
{
    public static string GetStatusText(RenoDXModStatus status) => status switch
    {
        RenoDXModStatus.Done => "Working",
        RenoDXModStatus.Wip => "WIP, may lack testing or have deal-breaking issues",
        _ => throw new UnreachableException($"No text for RenoDX mod status \"{status}\"")
    };

    public static string GetUnrealMethodText(RenoDXUnrealGenericMod.UnrealModMethod method) => method switch
    {
        RenoDXUnrealGenericMod.UnrealModMethod.Native =>
            "Uses the game's native HDR. Enable it in the game's settings and set Upgrade Path: Off in the RenoDX menu.",
        RenoDXUnrealGenericMod.UnrealModMethod.Ini =>
            "Needs the Engine.ini lines, with Upgrade Path: Off in the RenoDX menu.",
        RenoDXUnrealGenericMod.UnrealModMethod.Upgrade =>
            "Set Upgrade Path: On in the RenoDX menu and apply this game's resource upgrades.",
        _ => throw new UnreachableException($"No text for Unreal mod method \"{method}\"")
    };

    public static string GetNoticeText(RenoDXAvailability.Notice notice) => notice switch
    {
        RenoDXAvailability.Notice.EngineFallback =>
            "This game was not detected as being on the RenoDX mod list. The generic addon for this engine may work, " +
            "but it isn't guaranteed.",
        RenoDXAvailability.Notice.GameSpecificModAvailable =>
            "A mod made specifically for this game is now available! You are currently using the engine generic mod.",
        RenoDXAvailability.Notice.NoBuildForArchitecture =>
            "This mod has no build for your ReShade's bitness. " +
            "Please check that you have installed the correct ReShade (32-bit or 64-bit)",
        RenoDXAvailability.Notice.InstalledArchitectureMismatch =>
            "Your ReShade and RenoDX have different bitness, so RenoDX won't be able to load. " +
            "Please check your installed files.",
        RenoDXAvailability.Notice.NoDownloadListed =>
            "No download is listed for this mod yet.",
        RenoDXAvailability.Notice.SnapshotFileUnconfirmed =>
            "Couldn't confirm that the Snapshot release page contains this game's mod file. " +
            "You can still try installing it but it is not guaranteed to work.",
        RenoDXAvailability.Notice.AmbiguousMatch =>
            "This game matches more than one entry on the RenoDX mod list, so Restall can't tell which one to use. " +
            "Please report it with the logs so the list can be fixed.",
        _ => throw new UnreachableException($"No text for RenoDX notice \"{notice}\"")
    };
}
