using Moq;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Services;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Infrastructure;

public sealed class GameDetectionServiceTests
{
    // Verifies that game detection combines scanner results and enriches each valid game with engine data.
    [Fact]
    public async Task FindGamesAsync_CombinesScannerResultsAndSetsExecutablePathAndEngine()
    {
        var firstGame = CreateGame("Alpha", "c:\\games\\alpha", Game.Platform.Steam, "steam:1");
        var secondGame = CreateGame("Beta", "c:\\games\\beta", Game.Platform.GOG, "gog:2");
        var engineDetection = new Mock<IEngineDetectionService>(MockBehavior.Strict);
        engineDetection.Setup(x => x.DetectExecutablePathAndEngine("c:\\games\\alpha"))
            .Returns(("c:\\games\\alpha\\bin", Game.Engine.Unity));
        engineDetection.Setup(x => x.DetectExecutablePathAndEngine("c:\\games\\beta"))
            .Returns(("c:\\games\\beta\\bin", Game.Engine.Unreal));
        var sut = CreateSut(engineDetection, new[]
        {
            Scanner(Game.Platform.Steam, firstGame),
            Scanner(Game.Platform.GOG, secondGame)
        });

        var result = await sut.FindGamesAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "Alpha", "Beta" }, result.Games.Select(g => g.Name));
        Assert.Equal(Game.Engine.Unity, result.Games[0].EngineName);
        Assert.Equal("c:\\games\\alpha\\bin", result.Games[0].ExecutablePath);
        Assert.Equal(Game.Engine.Unreal, result.Games[1].EngineName);
        Assert.Equal("c:\\games\\beta\\bin", result.Games[1].ExecutablePath);
    }

    // Verifies that scanner progress is reported once per scanner.
    [Fact]
    public async Task FindGamesAsync_ReportsProgressForEachScanner()
    {
        var game = CreateGame("Alpha", "c:\\games\\alpha", Game.Platform.Steam, "steam:1");
        var reports = new List<GameScanProgressReportDto>();
        var progress = new Progress<GameScanProgressReportDto>(reports.Add);
        var engineDetection = new Mock<IEngineDetectionService>(MockBehavior.Strict);
        engineDetection.Setup(x => x.DetectExecutablePathAndEngine("c:\\games\\alpha"))
            .Returns(("c:\\games\\alpha", Game.Engine.Unknown));
        var sut = CreateSut(engineDetection, new[]
        {
            Scanner(Game.Platform.Steam, game),
            Scanner(Game.Platform.Epic)
        });

        await sut.FindGamesAsync(progress);

        Assert.Equal(2, reports.Count);
        Assert.Equal("Steam", reports[0].CompletedPlatform);
        Assert.Equal(1, reports[0].ScannersCompleted);
        Assert.Equal(2, reports[0].TotalScanners);
        Assert.Equal("Epic", reports[1].CompletedPlatform);
        Assert.Equal(2, reports[1].ScannersCompleted);
    }

    // Verifies that duplicate install folders are matched case-insensitively and prefer entries with platform ids.
    [Fact]
    public async Task FindGamesAsync_DeduplicatesInstallFoldersAndPrefersPlatformId()
    {
        var withoutId = CreateGame("Alpha No Id", "C:\\Games\\Alpha", Game.Platform.Unknown, platformId: null);
        var withId = CreateGame("Alpha With Id", "c:\\games\\alpha", Game.Platform.Steam, "steam:1");
        var engineDetection = new Mock<IEngineDetectionService>(MockBehavior.Strict);
        engineDetection.Setup(x => x.DetectExecutablePathAndEngine("c:\\games\\alpha"))
            .Returns(("c:\\games\\alpha", Game.Engine.Unknown));
        var sut = CreateSut(engineDetection, new[]
        {
            Scanner(Game.Platform.Unknown, withoutId),
            Scanner(Game.Platform.Steam, withId)
        });

        var result = await sut.FindGamesAsync();

        var game = Assert.Single(result.Games);
        Assert.Equal("Alpha With Id", game.Name);
        Assert.Equal("steam:1", game.PlatformId);
        engineDetection.Verify(x => x.DetectExecutablePathAndEngine(It.IsAny<string>()), Times.Once);
    }

    // Verifies that games without detected executable paths are filtered from results.
    [Fact]
    public async Task FindGamesAsync_FiltersGamesWithoutExecutablePath()
    {
        var valid = CreateGame("Valid", "c:\\games\\valid", Game.Platform.Steam, "steam:1");
        var invalid = CreateGame("Invalid", "c:\\games\\invalid", Game.Platform.Steam, "steam:2");
        var engineDetection = new Mock<IEngineDetectionService>(MockBehavior.Strict);
        engineDetection.Setup(x => x.DetectExecutablePathAndEngine("c:\\games\\valid"))
            .Returns(("c:\\games\\valid", Game.Engine.Unknown));
        engineDetection.Setup(x => x.DetectExecutablePathAndEngine("c:\\games\\invalid"))
            .Returns((null, Game.Engine.Unknown));
        var sut = CreateSut(engineDetection, new[] { Scanner(Game.Platform.Steam, valid, invalid) });

        var result = await sut.FindGamesAsync();

        var game = Assert.Single(result.Games);
        Assert.Equal("Valid", game.Name);
        Assert.True(result.IsSuccess);
    }

    // Verifies that scanner warning messages are aggregated in the final scan result.
    [Fact]
    public async Task FindGamesAsync_AggregatesScannerMessages()
    {
        var game = CreateGame("Alpha", "c:\\games\\alpha", Game.Platform.Steam, "steam:1");
        var engineDetection = new Mock<IEngineDetectionService>(MockBehavior.Strict);
        engineDetection.Setup(x => x.DetectExecutablePathAndEngine("c:\\games\\alpha"))
            .Returns(("c:\\games\\alpha", Game.Engine.Unknown));
        var sut = CreateSut(engineDetection, new[]
        {
            Scanner(Game.Platform.Steam, "Steam warning", game),
            Scanner(Game.Platform.Epic, "Epic warning")
        });

        var result = await sut.FindGamesAsync();

        Assert.Equal("Steam warning; Epic warning", result.Message);
    }

    // Verifies that scanner exceptions are converted into a safe failure result.
    [Fact]
    public async Task FindGamesAsync_WhenScannerThrows_ReturnsFailureWithGenericMessage()
    {
        var scanner = new Mock<IPlatformScannerService>(MockBehavior.Strict);
        scanner.SetupGet(x => x.Platform).Returns(Game.Platform.Steam);
        scanner.Setup(x => x.ScanAsync()).ThrowsAsync(new InvalidOperationException("boom"));
        var engineDetection = new Mock<IEngineDetectionService>(MockBehavior.Strict);
        var sut = CreateSut(engineDetection, new[] { scanner.Object });

        var result = await sut.FindGamesAsync();

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Games);
        Assert.Equal("Failed to scan game libraries. Please try rescanning.", result.Message);
    }

    private static GameDetectionService CreateSut(
        Mock<IEngineDetectionService> engineDetection,
        IEnumerable<IPlatformScannerService> scanners) =>
        new(new NoOpLogService(), scanners, engineDetection.Object);

    private static IPlatformScannerService Scanner(Game.Platform platform, params Game[] games) =>
        Scanner(platform, message: null, games);

    private static IPlatformScannerService Scanner(Game.Platform platform, string? message, params Game[] games)
    {
        var scanner = new Mock<IPlatformScannerService>(MockBehavior.Strict);
        scanner.SetupGet(x => x.Platform).Returns(platform);
        scanner.Setup(x => x.ScanAsync()).ReturnsAsync(new GameScanResultDto(
            platform,
            games,
            games.Length > 0,
            message));
        return scanner.Object;
    }

    private static Game CreateGame(string name, string installFolder, Game.Platform platform, string? platformId) =>
        new()
        {
            Name = name,
            InstallFolder = installFolder,
            PlatformName = platform,
            PlatformId = platformId
        };
}
