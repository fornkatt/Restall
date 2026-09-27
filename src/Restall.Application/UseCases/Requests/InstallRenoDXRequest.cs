// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases.Requests;

public record InstallRenoDXRequest(
    Game Game,
    Architecture Arch,
    RenoDX.Branch Branch,
    RenoDXModInfoDto? ModInfo = null,
    RenoDXGenericModInfoDto? GenericModInfo = null,
    string? TargetVersion = null);
