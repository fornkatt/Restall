// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Entities;
using System.Collections.Frozen;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

public class RenoDXTagInfoTests
{
    [Theory]
    [InlineData(2026, 10, 7, "20261007")]
    [InlineData(2027, 1, 5, "20270105")]
    public void Version_TagDate_ReturnsYearMonthDayDigits(int year, int month, int day, string expectedVersion)
    {
        var actual = CreateTag(new DateOnly(year, month, day));

        Assert.Equal(expectedVersion, actual.Version);
    }

    [Fact]
    public void HasAddonFileList_AddonFilenameListed_ReturnsTrue()
    {
        var actual = CreateTag(new DateOnly(2026, 10, 7),
            "renodx-game.addon64");

        Assert.True(actual.HasAddonFileList);
    }

    [Fact]
    public void HasAddonFileList_AssetPageUnavailable_ReturnsFalse()
    {
        var actual = CreateTag(new DateOnly(2026, 10, 7));

        Assert.False(actual.HasAddonFileList);
    }

    private static RenoDXTagInfo CreateTag(DateOnly date, params string[] addonFilenames) =>
        new(date, RenoDX.Branch.Snapshot, new Uri("https://restalltests.com/releases/"),
            addonFilenames.ToFrozenSet(StringComparer.OrdinalIgnoreCase));
}
