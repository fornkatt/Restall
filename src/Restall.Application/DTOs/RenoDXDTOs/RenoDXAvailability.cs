// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXAvailability(
    bool IsSupported,
    RenoDXAddonFile? File,
    RenoDXModLink? ManualSource,
    ImmutableArray<string> Notices,
    RenoDXGenericAddonInfo? GenericAddonInfo)
{
    public static readonly RenoDXAvailability NotSupported = new(false, null, null, [],
        null);
}
