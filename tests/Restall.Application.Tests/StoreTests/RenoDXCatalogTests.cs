// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Stores;
using Restall.Domain.Entities;

namespace Restall.Application.Tests.StoreTests;

public class RenoDXCatalogTests
{
    [Fact]
    public void RenoDXCatalog_BeforeFirstRefresh_ReturnsNothing()
    {
        var sut = new RenoDXCatalog();

        Assert.Equal(RenoDXModDatabase.Empty, sut.ModDatabase);
        Assert.Null(sut.Snapshot);
        Assert.Empty(sut.Nightlies);
    }

    [Fact]
    public void LoadModDatabase_SnapshotAlreadyLoaded_ReturnsNewDatabaseAndKeepsSnapshot()
    {
        var database = RenoDXModDatabase.Empty with
        {
            GameMods =
            [
                new RenoDXGameMod("Test Game", RenoDXModStatus.Done, null, null, null,
                    null, null, null, null)
            ]
        };
        var snapshot = new RenoDXTagInfo(new DateOnly(2026, 10, 5), RenoDX.Branch.Snapshot,
            new Uri("https://restalltests.com/snapshot/"), []);
        var sut = new RenoDXCatalog();

        sut.LoadSnapshot(snapshot);
        sut.LoadModDatabase(database);

        Assert.Same(database, sut.ModDatabase);
        Assert.Same(snapshot, sut.Snapshot);
        Assert.Empty(sut.Nightlies);
    }
}
