using Moq;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Application.UseCases;
using Restall.Domain.Entities;

namespace Restall.Tests.Application;

public sealed class RefreshLibraryUseCaseTests
{
    [Fact]
    public async Task ExecuteFullRescanAsync_BuildsSortedGameResultsWithDetectedModsCompatibilityAndUpdates()
    {
        var context = CreateContext();
        var alpha = new Game { Name = "Alpha Game", ExecutablePath = "alpha-path" };
        var dead = new Game { Name = "Dead Game", ExecutablePath = "dead-path" };
        var zulu = new Game { Name = "Zulu Game", ExecutablePath = "zulu-path" };
        var games = new[] { zulu, alpha, dead };

        var alphaReShade = new ReShade
        {
            BranchName = ReShade.Branch.Stable,
            SelectedFilename = "dxgi.dll",
            Version = "6.4.0"
        };
        var zuluRenoDX = new RenoDX
        {
            BranchName = RenoDX.Branch.Snapshot,
            OriginalName = "renodx-zulu.addon64",
            SelectedName = "renodx-zulu.addon64",
            Version = "20240101"
        };
        var alphaUpdate = new UpdateCheckResultDto(true, "6.4.0", "6.5.0");
        var zuluUpdate = new UpdateCheckResultDto(false, "20240101", "20240101");
        var zuluSpecificMod = new RenoDXModInfoDto(
            "Zulu Game",
            DiscordUrl: null,
            SnapshotUrl64: "https://example.test/renodx-zulu.addon64",
            SnapshotUrl32: null,
            NexusUrl: null,
            Maintainer: "Maintainer",
            Notes: null,
            Status: ":white_check_mark:");
        var deadSpecificMod = new RenoDXModInfoDto(
            "Dead Game",
            DiscordUrl: null,
            SnapshotUrl64: "https://example.test/renodx-dead.addon64",
            SnapshotUrl32: null,
            NexusUrl: null,
            Maintainer: "Maintainer",
            Notes: null,
            Status: "\ud83d\udc80");
        var alphaGenericMod = new RenoDXGenericModInfoDto(
            "Alpha Game",
            ":construction:",
            SupportedEngine.Unreal,
            Notes: "generic notes");

        context.GameDetection
            .Setup(x => x.FindGamesAsync(It.IsAny<IProgress<GameScanProgressReportDto>?>()))
            .ReturnsAsync(new GameScanResultDto(Game.Platform.Unknown, games, false, "partial scan warning"));
        context.ModCatalog.Setup(x => x.GetRenoDXWikiMods()).Returns(new[] { zuluSpecificMod, deadSpecificMod });
        context.ModCatalog.Setup(x => x.GetRenoDXGenericWikiMods()).Returns(new[] { alphaGenericMod });
        context.ModDetection.Setup(x => x.DetectInstalledReShadeAsync("alpha-path"))
            .ReturnsAsync(new HashSet<ReShade> { alphaReShade });
        context.ModDetection.Setup(x => x.DetectInstalledRenoDXAsync("zulu-path"))
            .ReturnsAsync(new HashSet<RenoDX> { zuluRenoDX });
        context.UpdateCheck.Setup(x => x.CheckReShadeUpdate(alphaReShade)).Returns(alphaUpdate);
        context.UpdateCheck.Setup(x => x.CheckRenoDXUpdate(zuluRenoDX)).Returns(zuluUpdate);

        var result = await context.Sut.ExecuteFullRescanAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal("partial scan warning", result.ErrorMessage);
        Assert.Equal(new[] { "Alpha Game", "Dead Game", "Zulu Game" }, result.Games.Select(g => g.Game.Name));

        var alphaResult = result.Games.Single(g => g.Game.Name == "Alpha Game");
        Assert.Same(alphaReShade, alphaResult.Game.ReShade);
        Assert.Null(alphaResult.Game.RenoDX);
        Assert.Same(alphaGenericMod, alphaResult.CompatibleGenericMod);
        Assert.Null(alphaResult.CompatibleMod);
        Assert.Same(alphaUpdate, alphaResult.ReShadeUpdateResult);
        Assert.Null(alphaResult.RenoDXUpdateResult);

        var deadResult = result.Games.Single(g => g.Game.Name == "Dead Game");
        Assert.Null(deadResult.CompatibleMod);
        Assert.Null(deadResult.CompatibleGenericMod);

        var zuluResult = result.Games.Single(g => g.Game.Name == "Zulu Game");
        Assert.Same(zuluRenoDX, zuluResult.Game.RenoDX);
        Assert.Same(zuluSpecificMod, zuluResult.CompatibleMod);
        Assert.Null(zuluResult.CompatibleGenericMod);
        Assert.Same(zuluUpdate, zuluResult.RenoDXUpdateResult);

        context.GameDetection.Verify(x => x.FindGamesAsync(It.IsAny<IProgress<GameScanProgressReportDto>?>()), Times.Once);
        context.VersionCatalog.Verify(x => x.FetchVersionsAsync(), Times.Once);
        context.ModCatalog.Verify(x => x.FetchModsAsync(), Times.Once);
        context.SteamGridDb.Verify(x => x.EnrichGameArtworkAsync(It.IsAny<Game>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ExecuteLightRescanAsync_UsesExistingGamesWithoutGameDetectionAndReturnsSuccessfulResult()
    {
        var context = CreateContext();
        var beta = new Game { Name = "Beta Game", ExecutablePath = "beta-path" };
        var alpha = new Game { Name = "Alpha Game", ExecutablePath = "alpha-path" };
        var alphaSpecificMod = new RenoDXModInfoDto(
            "Alpha Game",
            DiscordUrl: null,
            SnapshotUrl64: "https://example.test/renodx-alpha.addon64",
            SnapshotUrl32: null,
            NexusUrl: null,
            Maintainer: "Maintainer",
            Notes: null,
            Status: ":white_check_mark:");
        var betaRenoDX = new RenoDX
        {
            BranchName = RenoDX.Branch.Snapshot,
            OriginalName = "renodx-beta.addon64",
            SelectedName = "renodx-beta.addon64",
            Version = "20240101"
        };
        var betaUpdate = new UpdateCheckResultDto(true, "20240101", "20240202");

        context.ModCatalog.Setup(x => x.GetRenoDXWikiMods()).Returns(new[] { alphaSpecificMod });
        context.ModCatalog.Setup(x => x.GetRenoDXGenericWikiMods()).Returns(Array.Empty<RenoDXGenericModInfoDto>());
        context.ModDetection.Setup(x => x.DetectInstalledRenoDXAsync("beta-path"))
            .ReturnsAsync(new HashSet<RenoDX> { betaRenoDX });
        context.UpdateCheck.Setup(x => x.CheckRenoDXUpdate(betaRenoDX)).Returns(betaUpdate);

        var result = await context.Sut.ExecuteLightRescanAsync(new[] { beta, alpha });

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(new[] { "Alpha Game", "Beta Game" }, result.Games.Select(g => g.Game.Name));

        var alphaResult = result.Games.Single(g => g.Game.Name == "Alpha Game");
        Assert.Same(alphaSpecificMod, alphaResult.CompatibleMod);
        Assert.Null(alphaResult.CompatibleGenericMod);

        var betaResult = result.Games.Single(g => g.Game.Name == "Beta Game");
        Assert.Same(betaRenoDX, betaResult.Game.RenoDX);
        Assert.Same(betaUpdate, betaResult.RenoDXUpdateResult);

        context.GameDetection.Verify(x => x.FindGamesAsync(It.IsAny<IProgress<GameScanProgressReportDto>?>()), Times.Never);
        context.VersionCatalog.Verify(x => x.FetchVersionsAsync(), Times.Once);
        context.ModCatalog.Verify(x => x.FetchModsAsync(), Times.Once);
        context.SteamGridDb.Verify(x => x.EnrichGameArtworkAsync(It.IsAny<Game>()), Times.Exactly(2));
    }

    private static RefreshContext CreateContext()
    {
        var gameDetection = new Mock<IGameDetectionService>(MockBehavior.Strict);
        var steamGridDb = new Mock<ISteamGridDbService>(MockBehavior.Strict);
        var modDetection = new Mock<IModDetectionService>(MockBehavior.Strict);
        var updateCheck = new Mock<IUpdateCheckService>(MockBehavior.Strict);
        var versionCatalog = new Mock<IVersionCatalog>(MockBehavior.Strict);
        var modCatalog = new Mock<IModCatalog>(MockBehavior.Strict);

        versionCatalog.Setup(x => x.FetchVersionsAsync()).Returns(Task.CompletedTask);
        modCatalog.Setup(x => x.FetchModsAsync()).Returns(Task.CompletedTask);
        steamGridDb.Setup(x => x.EnrichGameArtworkAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);
        modDetection.Setup(x => x.DetectInstalledReShadeAsync(It.IsAny<string>()))
            .ReturnsAsync(new HashSet<ReShade>());
        modDetection.Setup(x => x.DetectInstalledRenoDXAsync(It.IsAny<string>()))
            .ReturnsAsync(new HashSet<RenoDX>());

        var sut = new RefreshLibraryUseCase(
            Mock.Of<ILogService>(),
            gameDetection.Object,
            steamGridDb.Object,
            modDetection.Object,
            updateCheck.Object,
            versionCatalog.Object,
            modCatalog.Object);

        return new RefreshContext(
            sut,
            gameDetection,
            steamGridDb,
            modDetection,
            updateCheck,
            versionCatalog,
            modCatalog);
    }

    private sealed record RefreshContext(
        RefreshLibraryUseCase Sut,
        Mock<IGameDetectionService> GameDetection,
        Mock<ISteamGridDbService> SteamGridDb,
        Mock<IModDetectionService> ModDetection,
        Mock<IUpdateCheckService> UpdateCheck,
        Mock<IVersionCatalog> VersionCatalog,
        Mock<IModCatalog> ModCatalog);
}
