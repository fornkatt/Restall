// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Helpers;

public static class RenoDXWikiModTypeHelper
{
    public static RenoDXWikiModType? GetFallbackModTypeFromEngine(Game.Engine engine) => engine switch
    {
        Game.Engine.Unreal => RenoDXWikiModType.UnrealExtended,
        Game.Engine.Unity => RenoDXWikiModType.Unity,
        _ => null
    };
}