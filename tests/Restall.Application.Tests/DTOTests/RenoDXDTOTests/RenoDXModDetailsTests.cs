// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

public class RenoDXModDetailsTests
{
    [Fact]
    public void ModDetailsEmpty_NoDatabaseEntry_ReturnsNothing()
    {
        var emptyDetails = RenoDXModDetails.Empty;

        Assert.Null(emptyDetails.ListedName);
        Assert.Null(emptyDetails.Status);
        Assert.Null(emptyDetails.Author);
        Assert.Null(emptyDetails.Notes);
        Assert.Null(emptyDetails.Method);
        Assert.Empty(emptyDetails.Upgrades);
        Assert.Empty(emptyDetails.Links);
    }
}
