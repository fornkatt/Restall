using Moq;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Infrastructure.Stores;

namespace Restall.Tests.Infrastructure;

public sealed class ModCatalogTests
{
    // Verifies that mod catalog getters are empty before the first fetch.
    [Fact]
    public void Getters_WhenCatalogHasNotFetched_ReturnEmptyLists()
    {
        var parse = new Mock<IParseService>(MockBehavior.Strict);
        var sut = new ModCatalog(parse.Object);

        Assert.Empty(sut.GetRenoDXWikiMods());
        Assert.Empty(sut.GetRenoDXGenericWikiMods());
    }

    // Verifies that FetchModsAsync stores both specific and generic RenoDX wiki mods.
    [Fact]
    public async Task FetchModsAsync_PopulatesSpecificAndGenericModLists()
    {
        var parse = new Mock<IParseService>(MockBehavior.Strict);
        var wikiMod = new RenoDXModInfoDto(
            "Game",
            DiscordUrl: null,
            SnapshotUrl64: "https://example.test/renodx-game.addon64",
            SnapshotUrl32: null,
            NexusUrl: null,
            Maintainer: "Maintainer",
            Notes: null,
            Status: ":white_check_mark:");
        var genericMod = new RenoDXGenericModInfoDto("Generic Unreal", ":white_check_mark:", SupportedEngine.Unreal);
        parse.Setup(x => x.FetchRenoDXWikiModsAsync())
            .ReturnsAsync(new RenoDXWikiParseResultDto(new[] { wikiMod }, new[] { genericMod }));
        var sut = new ModCatalog(parse.Object);

        await sut.FetchModsAsync();

        Assert.Equal(new[] { wikiMod }, sut.GetRenoDXWikiMods());
        Assert.Equal(new[] { genericMod }, sut.GetRenoDXGenericWikiMods());
    }

    // Verifies that a second fetch replaces stale mod catalog data.
    [Fact]
    public async Task FetchModsAsync_WhenCalledAgain_ReplacesPreviousLists()
    {
        var parse = new Mock<IParseService>(MockBehavior.Strict);
        var firstMod = new RenoDXModInfoDto(
            "First",
            DiscordUrl: null,
            SnapshotUrl64: "https://example.test/renodx-first.addon64",
            SnapshotUrl32: null,
            NexusUrl: null,
            Maintainer: "Maintainer",
            Notes: null,
            Status: ":white_check_mark:");
        parse.SetupSequence(x => x.FetchRenoDXWikiModsAsync())
            .ReturnsAsync(new RenoDXWikiParseResultDto(new[] { firstMod }, Array.Empty<RenoDXGenericModInfoDto>()))
            .ReturnsAsync(new RenoDXWikiParseResultDto(Array.Empty<RenoDXModInfoDto>(), Array.Empty<RenoDXGenericModInfoDto>()));
        var sut = new ModCatalog(parse.Object);

        await sut.FetchModsAsync();
        await sut.FetchModsAsync();

        Assert.Empty(sut.GetRenoDXWikiMods());
        Assert.Empty(sut.GetRenoDXGenericWikiMods());
    }
}
