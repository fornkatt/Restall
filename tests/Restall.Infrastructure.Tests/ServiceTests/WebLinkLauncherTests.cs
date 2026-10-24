// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Services;

namespace Restall.Infrastructure.Tests.ServiceTests;

public class WebLinkLauncherTests
{
    [Fact]
    public void OpenWebLink_RelativeUri_ThrowsArgumentException()
    {
        var sut = new WebLinkLauncher();

        Assert.Throws<ArgumentException>(() => sut.OpenWebLink(new Uri("relative/path", UriKind.Relative)));
    }
}
