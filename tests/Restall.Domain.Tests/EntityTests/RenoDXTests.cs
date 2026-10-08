// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;

namespace Restall.Domain.Tests.EntityTests;

public class RenoDXTests
{
    [Fact]
    public void BuildDate_VersionWithValidBuildDate_ReturnsDate()
    {
        var renoDX = new RenoDX { Version = "20261007" };

        Assert.Equal(new DateOnly(2026, 10, 7), renoDX.BuildDate);
    }

    [Theory]
    [InlineData("1.2.3")]
    [InlineData("20261306")]
    [InlineData("")]
    [InlineData(null)]
    public void BuildDate_VersionWithoutValidBuildDate_ReturnsNull(string? version)
    {
        var renoDX = new RenoDX { Version = version };

        Assert.Null(renoDX.BuildDate);
    }
}
