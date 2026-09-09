// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

﻿namespace Restall.Application.DTOs;

public record RenoDXGenericModInfoDto(
    string Name,
    string Status,
    RenoDXWikiModType RenoDxWikiModType,
    Architecture Architecture = Architecture.x64,
    string? Notes = null
)

{
    public string AddonFilename64 => GetAddonFilename("64");
    public string AddonFilename32 => GetAddonFilename("32");

    public bool SupportsX64 => !SupportsX32;
    public bool SupportsX32 => Architecture == Architecture.x32;

    public bool IsExternallyHosted => RenoDxWikiModType.IsExternallyHosted();

    public static string GetAddonFilename(RenoDXWikiModType renoDxWikiModType, string bit) =>
        renoDxWikiModType switch
        {
            RenoDXWikiModType.Unreal => $"renodx-unrealengine.addon{bit}",
            RenoDXWikiModType.UnrealExtended => $"renodx-ue-extended.addon{bit}",
            RenoDXWikiModType.Unity => $"renodx-unityengine.addon{bit}",
            _ => "unknown"
        };
    
    private string GetAddonFilename(string bit) => GetAddonFilename(RenoDxWikiModType, bit);
}

public enum Architecture
{
    x32 = 32,
    x64 = 64
}

public enum RenoDXWikiModType
{
    Unreal,
    UnrealExtended,
    Unity
}

public static class RenoDXWikiModTypeExtensions
{
    public static bool IsExternallyHosted(this RenoDXWikiModType renoDxWikiModType) =>
        renoDxWikiModType is RenoDXWikiModType.UnrealExtended or RenoDXWikiModType.Unity;
}