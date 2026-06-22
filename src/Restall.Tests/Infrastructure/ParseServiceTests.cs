using System.Net;
using Restall.Application.DTOs;
using Restall.Domain.Entities;
using Restall.Infrastructure.Services;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Infrastructure;

public sealed class ParseServiceTests
{
    // Verifies that ReShade versions are parsed, deduplicated and prefixed by a newer site version.
    [Fact]
    public async Task FetchReShadeVersionsAsync_ParsesTagsAndPrefixesNewerSiteVersion()
    {
        var sut = CreateService(new Dictionary<string, string>
        {
            ["https://github.com/crosire/reshade/tags"] = """
                <html><body>
                  <a href="/crosire/reshade/releases/tag/v6.4.0">v6.4.0</a>
                  <a href="/crosire/reshade/releases/tag/v6.4.0">duplicate</a>
                  <a href="/crosire/reshade/releases/tag/v6.3.0">v6.3.0</a>
                </body></html>
                """,
            ["https://reshade.me"] = "<html><body>Download ReShade 6.5.0 today</body></html>"
        });

        var result = await sut.FetchReShadeVersionsAsync();

        Assert.Equal(["6.5.0", "6.4.0", "6.3.0"], result);
    }

    // Verifies that ReShade HTTP failures are swallowed and return an empty list.
    [Fact]
    public async Task FetchReShadeVersionsAsync_WhenHttpFails_ReturnsEmptyList()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.TextResponse("fail", HttpStatusCode.InternalServerError));
        var sut = CreateService(handler);

        var result = await sut.FetchReShadeVersionsAsync();

        Assert.Empty(result);
    }

    // Verifies that snapshot releases parse date and commit notes from release HTML.
    [Fact]
    public async Task FetchRenoDXSnapshotAsync_ParsesReleaseDateAndCommitNotes()
    {
        var sut = CreateService(new Dictionary<string, string>
        {
            ["https://github.com/clshortfuse/renodx/releases/tag/snapshot"] = """
                <html><body>
                  <relative-time datetime="2024-02-03T12:00:00Z"></relative-time>
                  <div class="markdown-body">
                    <h2>Added</h2>
                    <ul><li>First change</li></ul>
                    <h2>Fixed</h2>
                    <ul><li>Second change</li></ul>
                  </div>
                </body></html>
                """
        });

        var result = await sut.FetchRenoDXSnapshotAsync();

        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2024, 2, 3), result.Date);
        Assert.Equal(RenoDX.Branch.Snapshot, result.Branch);
        Assert.Equal(["[Added] First change", "[Fixed] Second change"], result.CommitNotes);
    }

    // Verifies that snapshot releases without a parseable date return null.
    [Fact]
    public async Task FetchRenoDXSnapshotAsync_WhenDateIsMissing_ReturnsNull()
    {
        var sut = CreateService(new Dictionary<string, string>
        {
            ["https://github.com/clshortfuse/renodx/releases/tag/snapshot"] = "<html><body><div class=\"markdown-body\"></div></body></html>"
        });

        var result = await sut.FetchRenoDXSnapshotAsync();

        Assert.Null(result);
    }

    // Verifies that nightly tags and release notes are parsed while invalid nightly tags are ignored.
    [Fact]
    public async Task FetchRenoDXNightlyTagsAsync_ParsesNightlyTagsAndIgnoresInvalidDates()
    {
        var sut = CreateService(new Dictionary<string, string>
        {
            ["https://github.com/clshortfuse/renodx/tags"] = """
                <html><body>
                  <a href="/clshortfuse/renodx/releases/tag/nightly-20240203">nightly-20240203</a>
                  <a href="/clshortfuse/renodx/releases/tag/nightly-invalid">nightly-invalid</a>
                  <a href="/clshortfuse/renodx/releases/tag/nightly-20240202">nightly-20240202</a>
                  <a href="/clshortfuse/renodx/releases/tag/nightly-20240202">duplicate</a>
                </body></html>
                """,
            ["https://github.com/clshortfuse/renodx/releases/tag/nightly-20240203"] = """
                <html><body>
                  <pre class="text-small ws-pre-wrap">Header
                  First nightly note
                  Second nightly note</pre>
                </body></html>
                """,
            ["https://github.com/clshortfuse/renodx/releases/tag/nightly-20240202"] = "<html><body></body></html>"
        });

        var result = await sut.FetchRenoDXNightlyTagsAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("20240203", result[0].Version);
        Assert.Equal(["First nightly note", "Second nightly note"], result[0].CommitNotes);
        Assert.Equal("20240202", result[1].Version);
        Assert.Null(result[1].CommitNotes);
    }

    // Verifies that RenoDX wiki markdown parses specific and generic mod tables.
    [Fact]
    public async Task FetchRenoDXWikiModsAsync_ParsesSpecificAndGenericModTables()
    {
        var sut = CreateService(new Dictionary<string, string>
        {
            ["https://raw.githubusercontent.com/wiki/clshortfuse/renodx/Mods.md"] = """
                | Game | Maintainer | Links | Status |
                | --- | --- | --- | --- |
                | [Specific Game](https://example.test/game) | Alice | [x64](https://example.test/renodx-specific.addon64) [x32](https://example.test/renodx-specific.addon32) [Nexus](https://www.nexusmods.com/game) [Discord](https://discord.gg/game) | ok |

                ### Unreal Engine
                | Game | Status | Notes |
                | --- | --- | --- |
                | [Generic Unreal](https://example.test/unreal) | ok | Use for Unreal |

                ### Unity Engine
                | Game | Status | Notes |
                | --- | --- | --- |
                | Generic Unity | warning | |
                """
        });

        var result = await sut.FetchRenoDXWikiModsAsync();

        var wikiMod = Assert.Single(result.WikiMods);
        Assert.Equal("Specific Game", wikiMod.Name);
        Assert.Equal("Alice", wikiMod.Maintainer);
        Assert.Equal("https://example.test/renodx-specific.addon64", wikiMod.SnapshotUrl64);
        Assert.Equal("https://example.test/renodx-specific.addon32", wikiMod.SnapshotUrl32);
        Assert.Equal("https://www.nexusmods.com/game", wikiMod.NexusUrl);
        Assert.Equal("https://discord.gg/game", wikiMod.DiscordUrl);

        Assert.Equal(2, result.GenericWikiMods.Count);
        Assert.Equal(SupportedEngine.Unreal, result.GenericWikiMods[0].Engine);
        Assert.Equal("Use for Unreal", result.GenericWikiMods[0].Notes);
        Assert.Equal(SupportedEngine.Unity, result.GenericWikiMods[1].Engine);
        Assert.Null(result.GenericWikiMods[1].Notes);
    }

    // Verifies that RenoDX wiki HTTP failures return empty mod lists.
    [Fact]
    public async Task FetchRenoDXWikiModsAsync_WhenHttpFails_ReturnsEmptyLists()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.TextResponse("fail", HttpStatusCode.InternalServerError));
        var sut = CreateService(handler);

        var result = await sut.FetchRenoDXWikiModsAsync();

        Assert.Empty(result.WikiMods);
        Assert.Empty(result.GenericWikiMods);
    }

    private static ParseService CreateService(IReadOnlyDictionary<string, string> responses)
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            var uri = request.RequestUri!.ToString();
            var fallbackUri = uri.EndsWith("/", StringComparison.Ordinal) ? uri.TrimEnd('/') : uri;
            return (responses.TryGetValue(uri, out var content) || responses.TryGetValue(fallbackUri, out content))
                ? FakeHttpMessageHandler.TextResponse(content)
                : FakeHttpMessageHandler.TextResponse($"No fake response for {uri}", HttpStatusCode.NotFound);
        });

        return CreateService(handler);
    }

    private static ParseService CreateService(FakeHttpMessageHandler handler)
    {
        var client = new HttpClient(handler);
        return new ParseService(new NoOpLogService(), new FakeHttpClientFactory(client));
    }
}
