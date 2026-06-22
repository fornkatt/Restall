using Moq;
using Restall.Application.Interfaces.Driven;
using Restall.Infrastructure.Persistence;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Infrastructure;

public sealed class SteamGridDbIndexRepositoryTests
{
    // Verifies that missing index files produce an empty index.
    [Fact]
    public void TryGetSteamGridDbId_WhenIndexFileIsMissing_ReturnsNull()
    {
        using var temp = new TempDirectory();
        var sut = CreateRepository(temp);

        var result = sut.TryGetSteamGridDbId("steam:1");

        Assert.Null(result);
        Assert.True(Directory.Exists(temp.GetPath("sgdb")));
    }

    // Verifies that saved ids can be read back from the same repository instance.
    [Fact]
    public async Task SaveSteamGridDbIdAsync_SavesIdInMemory()
    {
        using var temp = new TempDirectory();
        var sut = CreateRepository(temp);

        await sut.SaveSteamGridDbIdAsync("steam:1", 123);

        Assert.Equal(123, sut.TryGetSteamGridDbId("steam:1"));
    }

    // Verifies that saved ids are persisted to index.json and loaded by a new repository instance.
    [Fact]
    public async Task SaveSteamGridDbIdAsync_PersistsIdToDisk()
    {
        using var temp = new TempDirectory();
        var first = CreateRepository(temp);

        await first.SaveSteamGridDbIdAsync("steam:1", 123);
        var second = CreateRepository(temp);

        Assert.Equal(123, second.TryGetSteamGridDbId("steam:1"));
        Assert.Contains("\"steam:1\"", File.ReadAllText(temp.GetPath("sgdb", "index.json")));
    }

    // Verifies that saving an existing cache key overwrites the previous id.
    [Fact]
    public async Task SaveSteamGridDbIdAsync_WhenKeyAlreadyExists_OverwritesId()
    {
        using var temp = new TempDirectory();
        var sut = CreateRepository(temp);

        await sut.SaveSteamGridDbIdAsync("steam:1", 123);
        await sut.SaveSteamGridDbIdAsync("steam:1", 456);

        Assert.Equal(456, sut.TryGetSteamGridDbId("steam:1"));
    }

    // Verifies that corrupt index files are ignored without throwing.
    [Fact]
    public void Constructor_WhenIndexFileIsCorrupt_LoadsEmptyIndexAndLogsError()
    {
        using var temp = new TempDirectory();
        temp.CreateFile(Path.Combine("sgdb", "index.json"), "{ invalid json");
        var log = new Mock<ILogService>(MockBehavior.Loose);

        var sut = CreateRepository(temp, log.Object);

        Assert.Null(sut.TryGetSteamGridDbId("steam:1"));
        log.Verify(x => x.LogError(
            It.Is<string>(message => message.Contains("Failed to load index file")),
            It.IsAny<Exception?>(),
            It.IsAny<string>()), Times.Once);
    }

    private static SteamGridDbIndexRepository CreateRepository(TempDirectory temp, ILogService? logService = null)
    {
        var pathService = new Mock<IPathService>(MockBehavior.Strict);
        pathService.Setup(x => x.GetSgdbCacheDirectory()).Returns(temp.GetPath("sgdb"));
        return new SteamGridDbIndexRepository(logService ?? new NoOpLogService(), pathService.Object);
    }
}
