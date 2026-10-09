// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging.Abstractions;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Services;
using Restall.Application.Stores;
using Restall.Application.Tests.Fakes;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;

namespace Restall.Application.Tests.ServiceTests;

public class GameEntryAssemblerTests
{
    [Fact]
    public void Assemble_GameWithReShadeAndListedMod_ReturnsEntryFromCatalogAndReShadeUpdate()
    {
        var catalog = new RenoDXCatalog();
        catalog.LoadModDatabase(RenoDXModDatabase.Empty with
        {
            GameMods =
            [
                new RenoDXGameMod("Test Game", RenoDXModStatus.Done, null, null,
                    null, null, null, null, null)
            ]
        });
        var reShadeUpdate = new UpdateAvailability(true, "6.3.0", "6.4.1");
        var updateCheckService = new FakeUpdateCheckService();
        updateCheckService.ReturnForReShade(reShadeUpdate);
        var sut = new GameEntryAssembler(NullLogger<GameEntryAssembler>.Instance, catalog, updateCheckService);
        var game = new Game { Name = "Test Game", ReShade = new ReShade { Arch = Architecture.X32 } };
        var actual = sut.Assemble(game);

        Assert.Same(game, actual.Game);
        Assert.Equal(Architecture.X32, actual.RecommendedArchitecture);
        Assert.Equal("Test Game", actual.RenoDXEntry.ListedName);
        Assert.Same(reShadeUpdate, actual.ReShadeUpdate);
    }
}
