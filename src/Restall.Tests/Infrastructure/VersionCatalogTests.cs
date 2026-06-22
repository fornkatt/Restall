using Moq;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Stores;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Infrastructure;

public sealed class VersionCatalogTests
{
    // Verifies that version fetch populates ReShade and RenoDX catalogs from the parse service.
    [Fact]
    public async Task FetchVersionsAsync_PopulatesAvailableVersionsAndLatestTags()
    {
        var parse = new Mock<IParseService>(MockBehavior.Strict);
        var snapshot = new RenoDXTagInfoDto(new DateOnly(2024, 2, 2), RenoDX.Branch.Snapshot);
        var nightly = new RenoDXTagInfoDto(new DateOnly(2024, 2, 1), RenoDX.Branch.Nightly);
        parse.Setup(x => x.FetchReShadeVersionsAsync()).ReturnsAsync(new[] { "6.5.0", "6.4.0" });
        parse.Setup(x => x.FetchRenoDXSnapshotAsync()).ReturnsAsync(snapshot);
        parse.Setup(x => x.FetchRenoDXNightlyTagsAsync()).ReturnsAsync(new[] { nightly });
        var sut = new VersionCatalog(parse.Object, new NoOpLogService());

        await sut.FetchVersionsAsync();

        Assert.Equal("6.5.0", sut.GetLatestReShadeVersion(ReShade.Branch.Stable));
        Assert.Equal(new[] { "6.5.0", "6.4.0" }, sut.GetAvailableReShadeVersions(ReShade.Branch.Stable));
        Assert.Same(snapshot, sut.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot));
        Assert.Same(nightly, sut.GetLatestRenoDXVersionByTag(RenoDX.Branch.Nightly));
        Assert.Equal(new[] { nightly }, sut.GetAllRenoDXNightlies());
    }

    // Verifies that fetching versions clears stale catalog data before repopulating.
    [Fact]
    public async Task FetchVersionsAsync_WhenCalledAgain_ClearsStaleValues()
    {
        var parse = new Mock<IParseService>(MockBehavior.Strict);
        var firstSnapshot = new RenoDXTagInfoDto(new DateOnly(2024, 2, 2), RenoDX.Branch.Snapshot);

        parse.SetupSequence(x => x.FetchReShadeVersionsAsync())
            .ReturnsAsync(new[] { "6.5.0" })
            .ReturnsAsync(Array.Empty<string>());
        parse.SetupSequence(x => x.FetchRenoDXSnapshotAsync())
            .ReturnsAsync(firstSnapshot)
            .ReturnsAsync((RenoDXTagInfoDto?)null);
        parse.SetupSequence(x => x.FetchRenoDXNightlyTagsAsync())
            .ReturnsAsync(new[] { new RenoDXTagInfoDto(new DateOnly(2024, 2, 1), RenoDX.Branch.Nightly) })
            .ReturnsAsync(Array.Empty<RenoDXTagInfoDto>());
        var sut = new VersionCatalog(parse.Object, new NoOpLogService());

        await sut.FetchVersionsAsync();
        await sut.FetchVersionsAsync();

        Assert.Null(sut.GetLatestReShadeVersion(ReShade.Branch.Stable));
        Assert.Empty(sut.GetAvailableReShadeVersions(ReShade.Branch.Stable));
        Assert.Null(sut.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot));
        Assert.Null(sut.GetLatestRenoDXVersionByTag(RenoDX.Branch.Nightly));
        Assert.Empty(sut.GetAllRenoDXNightlies());
    }

    // Verifies that unknown catalog branches return empty values before any fetch.
    [Fact]
    public void Getters_WhenCatalogIsEmpty_ReturnEmptyValues()
    {
        var parse = new Mock<IParseService>(MockBehavior.Strict);
        var sut = new VersionCatalog(parse.Object, new NoOpLogService());

        Assert.Null(sut.GetLatestReShadeVersion(ReShade.Branch.Stable));
        Assert.Empty(sut.GetAvailableReShadeVersions(ReShade.Branch.Stable));
        Assert.Null(sut.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot));
        Assert.Empty(sut.GetAllRenoDXNightlies());
    }

    // Verifies that missing snapshot data does not create a Snapshot catalog entry.
    [Fact]
    public async Task FetchVersionsAsync_WhenSnapshotIsMissing_LeavesSnapshotLatestNull()
    {
        var parse = new Mock<IParseService>(MockBehavior.Strict);
        parse.Setup(x => x.FetchReShadeVersionsAsync()).ReturnsAsync(new[] { "6.5.0" });
        parse.Setup(x => x.FetchRenoDXSnapshotAsync()).ReturnsAsync((RenoDXTagInfoDto?)null);
        parse.Setup(x => x.FetchRenoDXNightlyTagsAsync()).ReturnsAsync(Array.Empty<RenoDXTagInfoDto>());
        var sut = new VersionCatalog(parse.Object, new NoOpLogService());

        await sut.FetchVersionsAsync();

        Assert.Null(sut.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot));
    }
}
