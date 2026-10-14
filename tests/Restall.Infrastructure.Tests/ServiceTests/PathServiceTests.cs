// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Services;

namespace Restall.Infrastructure.Tests.ServiceTests;

public class PathServiceTests
{
    private readonly PathService _sut = new();

    [Fact]
    public void GetRenoDXSnapshotDownloadPath_SnapshotVersion_ReturnsPathInVersionFolder()
    {
        var actual = _sut.GetRenoDXSnapshotDownloadPath("20261005", "renodx-game.addon64");

        Assert.EndsWith(Path.Combine("DownloadCache", "RenoDX", "Snapshot", "20261005", "renodx-game.addon64"),
            actual);
    }

    [Fact]
    public void GetRenoDXNightlyDownloadPath_NightlyVersion_ReturnsPathInVersionFolder()
    {
        var actual = _sut.GetRenoDXNightlyDownloadPath("20261005", "renodx-game.addon64");

        Assert.EndsWith(Path.Combine("DownloadCache", "RenoDX", "Nightly", "20261005", "renodx-game.addon64"),
            actual);
    }
}
