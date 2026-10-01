// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;
using System.Collections.Frozen;

namespace Restall.Application.DTOs;

public sealed record GameEntry(
    Game Game,
    Architecture RecommendedArchitecture,
    RenoDXAvailability RenoDX,
    RenoDXModDetails RenoDXDetails,
    FrozenDictionary<RenoDX.Branch, UpdateCheck> RenoDXUpdates,
    UpdateCheck? ReShadeUpdate);
