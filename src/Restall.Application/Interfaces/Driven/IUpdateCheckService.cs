// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IUpdateCheckService
{
    UpdateCheck CheckReShadeUpdate(ReShade installed);
    UpdateCheck CheckRenoDXUpdate(RenoDX installed);
}
