// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging.Abstractions;
using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Services;
using Restall.Application.Stores;
using Restall.Application.Tests.Fakes;
using Restall.Application.UseCases;
using Restall.Domain.Entities;

namespace Restall.Application.Tests.UseCaseTests;

public class FullLibraryRefreshUseCaseTests
{
    private readonly FakeGameDetectionService _gameDetectionService = new();
    private readonly FakeParseService _parseService = new();
    private readonly FakeRenoDXModSourceService _renoDXModSourceService = new();
    private readonly RenoDXCatalog _renoDXCatalog = new();

    private static readonly RenoDXTagInfo s_snapshot = new(new DateOnly(2026, 10, 10),
        new Uri("https://restalltests.com/snapshot/"), ["renodx-game.addon64"], []);

    private static readonly RenoDXGameMod s_testGameMod =
        new("Test Game", RenoDXModStatus.Done, null, null, null, null,
            null, null, null);

    private static readonly RenoDXUnrealGenericMod s_unrealGenericMod =
        new("Unreal Generic Mod", RenoDXModStatus.Done, RenoDXUnrealGenericMod.UnrealModMethod.Native,
            null, null);

    private static readonly RenoDXUnityGenericMod s_unityGenericMod =
        new("Unity Generic Mod", RenoDXModStatus.Done, null, null);

    [Fact]
    public async Task ExecuteAsync_AllFetchesSucceed_ReturnsEntryBuiltFromFetchedModList()
    {
        ArrangeSuccessfulRefresh();
        var sut = CreateUseCase();

        var actual = await sut.ExecuteAsync();
        var entry = Assert.Single(actual.GameEntries);

        Assert.True(actual.IsSuccess);
        Assert.Empty(actual.Warnings);
        Assert.Null(actual.ErrorMessage);
        Assert.Equal("Test Game", entry.Game.Name);
        Assert.Equal(RenoDXTexts.GetListedAsText("Test Game"), entry.RenoDXEntry.HeadingText);
    }

    [Fact]
    public async Task ExecuteAsync_SnapshotFetchFails_ReturnsRefreshWarningAndKeepsLastLoadedSnapshot()
    {
        ArrangeSuccessfulRefresh();
        var lastLoadedSnapshot = new RenoDXTagInfo(new DateOnly(2026, 10, 1),
            new Uri("https://restalltests.com/old-snapshot/"), [], []);
        _renoDXCatalog.LoadSnapshot(lastLoadedSnapshot);
        _parseService.ReturnSnapshotFailure();
        var sut = CreateUseCase();

        var actual = await sut.ExecuteAsync();

        Assert.True(actual.IsSuccess);
        Assert.Equal("Couldn't refresh the RenoDX Snapshot release. Try refreshing again later.",
            Assert.Single(actual.Warnings));
        Assert.Same(lastLoadedSnapshot, _renoDXCatalog.Snapshot);
    }

    [Fact]
    public async Task ExecuteAsync_SnapshotWithoutFileList_ReturnsFileListWarningAndLoadsSnapshot()
    {
        ArrangeSuccessfulRefresh();
        var snapshotWithoutFileList = new RenoDXTagInfo(new DateOnly(2026, 10, 1),
            new Uri("https://restalltests.com/snapshot/"), [], []);
        _parseService.ReturnPartialSnapshot(snapshotWithoutFileList, WarningType.RenoDXSnapshotFileListUnavailable);
        var sut = CreateUseCase();

        var actual = await sut.ExecuteAsync();

        Assert.Equal("Couldn't read the file list of the RenoDX Snapshot release, so it can't confirm which mods it " +
                     "contains.", Assert.Single(actual.Warnings));
        Assert.Same(snapshotWithoutFileList, _renoDXCatalog.Snapshot);
    }

    [Fact]
    public async Task ExecuteAsync_GameModsFetchFails_ReturnsRefreshWarningAndKeepsLastLoadedGameMods()
    {
        ArrangeSuccessfulRefresh();
        var lastLoadedGameMod = s_testGameMod with { Author = "Earlier Author" };
        var lastLoadedUnrealGenericMod = s_unrealGenericMod with { Name = "Earlier Unreal Generic Mod" };
        _renoDXCatalog.LoadModDatabase(new RenoDXModDatabase([lastLoadedGameMod], [lastLoadedUnrealGenericMod],
            [s_unityGenericMod]));
        _renoDXModSourceService.ReturnGameModsFailure();
        var sut = CreateUseCase();

        var actual = await sut.ExecuteAsync();

        Assert.Equal("Couldn't refresh the RenoDX mod list. Try refreshing again later.",
            Assert.Single(actual.Warnings));
        Assert.Equal(lastLoadedGameMod, Assert.Single(_renoDXCatalog.ModDatabase.GameMods));
        Assert.Equal(s_unrealGenericMod, Assert.Single(_renoDXCatalog.ModDatabase.UnrealGenericMods));
    }

    [Fact]
    public async Task ExecuteAsync_GameScanFails_ReturnsScanErrorMessageAndStillLoadsSnapshot()
    {
        ArrangeSuccessfulRefresh();
        _gameDetectionService.ReturnScanFailure("Failed to scan game libraries. Please try rescanning.");
        var sut = CreateUseCase();

        var actual = await sut.ExecuteAsync();

        Assert.False(actual.IsSuccess);
        Assert.Equal("Failed to scan game libraries. Please try rescanning.", actual.ErrorMessage);
        Assert.Same(s_snapshot, _renoDXCatalog.Snapshot);
    }

    private void ArrangeSuccessfulRefresh()
    {
        var testGame = new Game
        {
            Name = "Test Game",
            ExecutablePath = Path.Combine("games", "Test Game", "TestGame.exe")
        };

        _gameDetectionService.ReturnGames(testGame);
        _parseService.ReturnReShadeVersions(["6.5.0"]);
        _parseService.ReturnSnapshot(s_snapshot);
        _parseService.ReturnNightlies([]);
        _renoDXModSourceService.ReturnGameMods([s_testGameMod]);
        _renoDXModSourceService.ReturnUnrealGenericMods([s_unrealGenericMod]);
        _renoDXModSourceService.ReturnUnityGenericMods([s_unityGenericMod]);
    }

    private FullLibraryRefreshUseCase CreateUseCase() =>
        new(NullLogger<FullLibraryRefreshUseCase>.Instance,
            _gameDetectionService,
            new FakeGameArtworkService(),
            new FakeModDetectionService(),
            new FakeVersionCatalog(),
            _parseService,
            _renoDXModSourceService,
            new RenoDXModDatabaseBuilder(NullLogger<RenoDXModDatabaseBuilder>.Instance),
            _renoDXCatalog,
            new GameEntryAssembler(NullLogger<GameEntryAssembler>.Instance, _renoDXCatalog,
                new FakeUpdateCheckService()));
}
