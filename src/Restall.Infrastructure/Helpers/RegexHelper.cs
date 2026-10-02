// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.RegularExpressions;

namespace Restall.Infrastructure.Helpers;

internal static partial class RegexHelper
{
    internal static Regex RenoDXVersionRegex => RenoDXVersion();
    internal static Regex ExtractReShadeVersionFromSite => ExtractReShadeFromSite();
    internal static Regex SteamLibraryRegex => SteamLibrary();
    internal static Regex Match32BitRegex => Match32Bit();

    [GeneratedRegex(@"\b32[\s-]?bit\b", RegexOptions.IgnoreCase)]
    private static partial Regex Match32Bit();

    [GeneratedRegex(@"^\d+\.(\d{4})\.(\d{4})\.\d+$")]
    private static partial Regex RenoDXVersion();

    [GeneratedRegex(@"ReShade (\d+\.\d+\.\d+)")]
    private static partial Regex ExtractReShadeFromSite();

    [GeneratedRegex(@"""path""\s+""([^""]+)""")]
    private static partial Regex SteamLibrary();

}
