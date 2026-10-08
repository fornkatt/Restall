// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;

namespace Restall.Application.Common;

public static class RenoDXGenericAddons
{
    private const string UnityAddonDownloadBaseUrl = "https://notvoosh.github.io/renodx-unity/";

    private const string UnrealExtendedAddonFilename = "renodx-ue-extended" + RenoDX.AddonExtension;
    private const string UnityAddonFilename = "renodx-unityengine" + RenoDX.AddonExtension;
    private const string UnityAddonFilename32 = "renodx-unityengine" + RenoDX.AddonExtension32;

    public static string? GetUnrealExtendedAddonFilename(Architecture architecture) =>
        architecture == Architecture.X64 ? UnrealExtendedAddonFilename : null;

    public static string GetUnityGenericAddonFilename(Architecture architecture) =>
        architecture == Architecture.X32 ? UnityAddonFilename32 : UnityAddonFilename;

    public static Uri? GetGenericAddonDownloadUrl(string addonFilename) => addonFilename.ToLowerInvariant() switch
    {
        UnrealExtendedAddonFilename => s_unrealExtendedDownloadUrl,
        UnityAddonFilename => s_unityAddonDownloadUrl,
        UnityAddonFilename32 => s_unityAddonDownloadUrl32,
        _ => null
    };

    public static RenoDXGenericAddonInfo? GetGenericAddonInfo(string addonFilename) =>
        addonFilename.ToLowerInvariant() switch
        {
            UnrealExtendedAddonFilename => s_unrealExtendedInfo,
            UnityAddonFilename or UnityAddonFilename32 => s_unityGenericInfo,
            _ => null
        };

    private static readonly Uri s_unrealExtendedDownloadUrl =
        new($"https://marat569.github.io/renodx/{UnrealExtendedAddonFilename}");

    private static readonly Uri s_unityAddonDownloadUrl =
        new($"{UnityAddonDownloadBaseUrl}{UnityAddonFilename}");

    private static readonly Uri s_unityAddonDownloadUrl32 =
        new($"{UnityAddonDownloadBaseUrl}{UnityAddonFilename32}");

    private static readonly RenoDXGenericAddonInfo s_unrealExtendedInfo =
        new("UE Extended", "Marat", UnrealExtendedNotes, UnrealExtendedEngineIni);

    private static readonly RenoDXGenericAddonInfo s_unityGenericInfo =
        new("Unity Generic", "Voosh", UnityNotes, null);

    private const string UnrealExtendedNotes =
        """
        Unreal Extended is a rework of the original generic Unreal Engine mod. It is 64-bit only.
        This improved version offers increased stability and working Frame Generation without the need for workarounds.

        If the game has native HDR, enable it and set "Upgrade Path: Off" in the RenoDX menu.

        Unreal Engine 5 games without native HDR support may get it through the Engine.ini lines below.
        These tweaks are not recommended for UE4.
        First, set "Upgrade Path: Off" and add the lines to Engine.ini in the game's config folder.
        It's usually located at "%localappdata%\[GameName]\Saved\Config\Windows" on Windows.
        Usually [WINEPrefix]/drive_c/users/[UserName]/AppData/Local/[GameName]/Saved/Config/Windows on Linux.

        If these cause issues, disable native HDR, revert Engine.ini and set "Upgrade Path: On" with resource upgrades if needed.

        Order to try: Native HDR, then Engine.ini, then "Upgrade Path: On".
        """;

    private const string UnrealExtendedEngineIni =
        """
        #Enables UE HDR for HDR path in RenoDX, you don't need this for Upgrade Path: On!
        [SystemSettings]
        r.AllowHDR=1
        r.HDR.EnableHDROutput=1
        r.HDR.Display.OutputDevice=3
        r.HDR.Display.ColorGamut=2
        r.HDR.UI.CompositeMode=1

        #Enables real-time RenoDX sliders after UE5.3
        [/Script/Engine.RendererSettings]
        r.LUT.UpdateEveryFrame=1
        """;

    private const string UnityNotes =
        """
        The Unity generic addon is a generic RenoDX addon for Unity games. It's available for both 64-bit and 32-bit.

        If colors stay limited to BT.709, upgrade "R11G11B10_FLOAT" then restart the game for it to take effect.
        The upgrades appear after switching Settings Mode to Advanced in the RenoDX menu.

        Avoid exclusive fullscreen.
        The game's brightness/contrast/gamma settings should be left at default in most cases.
        Crashes when using XeSS -> update "libxess.dll" in the game folder.
        """;
}
