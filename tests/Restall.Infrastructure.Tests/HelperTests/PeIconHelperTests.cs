// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Helpers;

namespace Restall.Infrastructure.Tests.HelperTests;

public class PeIconHelperTests
{
    [Fact]
    public void ExtractLargestIconAsPng_ManagedAssemblyWithoutIcon_ReturnsNothing()
    {
        var actual = PeIconHelper.ExtractLargestIconAsPng(typeof(PeIconHelper).Assembly.Location);

        Assert.Null(actual);
    }
}
