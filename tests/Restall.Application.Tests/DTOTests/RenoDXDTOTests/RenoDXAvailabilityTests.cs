// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

public class RenoDXAvailabilityTests
{
    [Fact]
    public void AvailabilityNotSupported_NoDatabaseEntryOrFallback_ReturnsNothing()
    {
        var availability = RenoDXAvailability.NotSupported;

        Assert.False(availability.IsSupported);
        Assert.Null(availability.DownloadOptions);
        Assert.Null(availability.ManualSource);
        Assert.Empty(availability.Notices);
        Assert.Null(availability.GenericAddonInfo);
    }
}
