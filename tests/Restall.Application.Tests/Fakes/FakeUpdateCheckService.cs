// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Application.Tests.Fakes;

public sealed class FakeUpdateCheckService : IUpdateCheckService
{
    private UpdateAvailability _reShadeUpdate = new(false, null, null);

    public void ReturnForReShade(UpdateAvailability reShadeUpdate) => _reShadeUpdate = reShadeUpdate;

    public UpdateAvailability CheckReShadeUpdate(ReShade installed) => _reShadeUpdate;

    public UpdateAvailability CheckRenoDXUpdate(RenoDX installed) =>
        throw new NotSupportedException("RenoDX updates come from RenoDXEntry.BranchVersions");
}
