// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging.Abstractions;
using Restall.Application.Common.Enums;
using Restall.Infrastructure.Services;
using Restall.Infrastructure.Tests.Fakes;
using System.Net;
using System.Text;

namespace Restall.Infrastructure.Tests.ServiceTests;

public class ParseServiceTests
{
    private const string RenoDXSnapshotPageUrl = "https://github.com/clshortfuse/renodx/releases/tag/snapshot";
    private const string RenoDXAssetsUrl = "https://github.com/clshortfuse/renodx/releases/expanded_assets/";

    private const string RenoDXSnapshotPage =
        """
        <html>
            <body>
                <relative-time datetime="2026-10-03T00:56:23Z">Oct 3</relative-time>
                <div class="markdown-body">
                    <h2>Changes</h2>
                    <ul>
                        <li>Fix test game</li>
                    </ul>
                </div>
            </body>
        </html>
        """;

    private const string RenoDXTagsPage =
        """
        <html>
            <body>
                <a href="/clshortfuse/renodx/releases/tag/nightly-20261002">nightly-20261002</a>
                <a href="/clshortfuse/renodx/releases/tag/nightly-20261003">nightly-20261003</a>
            </body>
        </html>
        """;

    private const string RenoDXNightlyPage =
        """
        <html>
            <body>
                <pre class="text-small ws-pre-wrap">nightly
                Fix test game</pre>
            </body>
        </html>
        """;

    [Fact]
    public async Task FetchRenoDXSnapshotAsync_PagesAvailable_ReturnsSuccessWithAddonFilenames()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(RenoDXSnapshotPageUrl, HtmlResponse(RenoDXSnapshotPage));
        handler.RespondWith(RenoDXAssetsUrl + "snapshot", HtmlResponse(AssetsPage("snapshot")));
        var sut = CreateService(handler);

        var actual = await sut.FetchRenoDXSnapshotAsync(TestContext.Current.CancellationToken);

        Assert.True(actual.IsSuccess);
        Assert.False(actual.IsPartial);
        Assert.NotNull(actual.Value);
        Assert.Equal("20261003", actual.Value.Version);
        Assert.Equal(["renodx-game.addon64", "renodx-game.addon32"], actual.Value.AddonFilenames);
        Assert.Equal(new Uri("https://github.com/clshortfuse/renodx/releases/download/snapshot/renodx-game.addon64"),
            actual.Value.GetDownloadUrl("renodx-game.addon64"));
    }

    [Fact]
    public async Task FetchRenoDXSnapshotAsync_AddonFileListUnavailable_ReturnsPartialWithNoAddonFilenames()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(RenoDXSnapshotPageUrl, HtmlResponse(RenoDXSnapshotPage));
        handler.RespondWith(RenoDXAssetsUrl + "snapshot", new HttpResponseMessage(HttpStatusCode.NotFound));
        var sut = CreateService(handler);

        var actual = await sut.FetchRenoDXSnapshotAsync(TestContext.Current.CancellationToken);

        Assert.True(actual.IsSuccess);
        Assert.True(actual.IsPartial);
        Assert.Equal(WarningType.RenoDXSnapshotFileListUnavailable, actual.WarningType);
        Assert.NotNull(actual.Value);
        Assert.Equal("20261003", actual.Value.Version);
        Assert.Empty(actual.Value.AddonFilenames);
    }

    [Fact]
    public async Task FetchRenoDXSnapshotAsync_ReleasePageUnreachable_ReturnsError()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(RenoDXSnapshotPageUrl, new HttpResponseMessage(HttpStatusCode.NotFound));
        var sut = CreateService(handler);

        var actual = await sut.FetchRenoDXSnapshotAsync(TestContext.Current.CancellationToken);

        Assert.False(actual.IsSuccess);
        Assert.False(actual.IsPartial);
        Assert.IsType<HttpRequestException>(actual.Exception);
        Assert.Null(actual.Value);
    }

    [Fact]
    public async Task FetchRenoDXNightlyTagsAsync_PagesAvailable_ReturnsSuccessWithEveryNightly()
    {
        var handler = CreateNightlyHandler();
        var sut = CreateService(handler);

        var actual = await sut.FetchRenoDXNightlyTagsAsync(TestContext.Current.CancellationToken);

        Assert.True(actual.IsSuccess);
        Assert.False(actual.IsPartial);
        Assert.Equal(["20261002", "20261003"], actual.Value.Select(n => n.Version));
        Assert.Equal(["renodx-game.addon64", "renodx-game.addon32"], actual.Value[0].AddonFilenames);
    }

    [Fact]
    public async Task FetchRenoDXNightlyTagsAsync_OneAddonFileListUnavailable_ReturnsPartialWithTheOtherNightlies()
    {
        var handler = CreateNightlyHandler();
        handler.RespondWith(RenoDXAssetsUrl + "nightly-20261002",
            new HttpResponseMessage(HttpStatusCode.NotFound));
        var sut = CreateService(handler);

        var actual = await sut.FetchRenoDXNightlyTagsAsync(TestContext.Current.CancellationToken);

        Assert.True(actual.IsSuccess);
        Assert.True(actual.IsPartial);
        Assert.Equal(WarningType.RenoDXNightliesIncomplete, actual.WarningType);
        Assert.Equal("20261003", Assert.Single(actual.Value).Version);
    }

    private static FakeHttpMessageHandler CreateNightlyHandler()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith("https://github.com/clshortfuse/renodx/tags", HtmlResponse(RenoDXTagsPage));

        foreach (var tag in new[] { "nightly-20261002", "nightly-20261003" })
        {
            handler.RespondWith("https://github.com/clshortfuse/renodx/releases/tag/" + tag,
                HtmlResponse(RenoDXNightlyPage));
            handler.RespondWith($"https://github.com/clshortfuse/renodx/releases/expanded_assets/{tag}",
                HtmlResponse(AssetsPage(tag)));
        }

        return handler;
    }

    private static string AssetsPage(string tag) =>
        $"""
         <html>
             <body>
                 <a href="/clshortfuse/renodx/releases/download/{tag}/renodx-game.addon64">renodx-game.addon64</a>
                 <a href="/clshortfuse/renodx/releases/download/{tag}/renodx-game.addon32">renodx-game.addon32</a>
                 <a href="/clshortfuse/renodx/releases/download/{tag}/analyze_shader_deps.exe">analyze_shader_deps.exe</a>
             </body>
         </html>
         """;

    private static ParseService CreateService(FakeHttpMessageHandler handler) =>
        new(NullLogger<ParseService>.Instance, new FakeHttpClientFactory(handler));

    private static HttpResponseMessage HtmlResponse(string html) =>
        new(HttpStatusCode.OK) { Content = new StringContent(html, Encoding.UTF8, "text/html") };
}
