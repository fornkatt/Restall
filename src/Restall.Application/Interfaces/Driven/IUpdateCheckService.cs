// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.Results;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IUpdateCheckService
{
    UpdateCheckResultDto CheckReShadeUpdate(ReShade installed);
    UpdateCheckResultDto CheckRenoDXUpdate(RenoDX installed);
}
