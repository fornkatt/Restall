// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging.Abstractions;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Infrastructure.Services;
using Restall.Infrastructure.Tests.Fakes;
using System.Net;
using System.Text;

namespace Restall.Infrastructure.Tests.Services;

public class RenoDXModSourceServiceTests
{
    private const string ValidGameModEntry =
        """
        {
           "name": "Test Game",
           "status": "Done",
           "author": "Tester",
           "snapshotUrl": "https://restalltests.com/renodx-game.addon64",
           "snapshotUrl32": "https://restalltests.com/renodx-game.addon32",
           "nexusUrl": null,
           "discordUrl": null,
           "discussionUrl": null,
           "notes": "test game"
        }
        """;

    private const string InvalidGameModEntry =
        """
        {
           "name": "Broken Game",
           "status": "Broken",
           "author": null,
           "snapshotUrl": null,
           "snapshotUrl32": null,
           "nexusUrl": null,
           "discordUrl": null,
           "discussionUrl": null,
           "notes": null
        }
        """;

    [Fact]
    public async Task FetchGameModsAsync_ValidEntry_ReturnsSuccessWithEveryField()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(RenoDXModSourceService.GameModsUrl, JsonResponse($"[{ValidGameModEntry}]"));
        var sut = CreateService(handler);

        var result = await sut.FetchGameModsAsync(TestContext.Current.CancellationToken);
        var actual = Assert.Single(result.Value);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsPartial);
        Assert.Equal(new RenoDXGameMod("Test Game",
            RenoDXModStatus.Done,
            "Tester",
            "https://restalltests.com/renodx-game.addon64",
            "https://restalltests.com/renodx-game.addon32",
            null,
            null,
            null,
            "test game"), actual);
    }

    [Fact]
    public async Task FetchGameModsAsync_InvalidAndValidEntry_ReturnsPartialWithOnlyReadableEntries()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(RenoDXModSourceService.GameModsUrl,
            JsonResponse($"[{ValidGameModEntry}, {InvalidGameModEntry}]"));
        var sut = CreateService(handler);

        var result = await sut.FetchGameModsAsync(TestContext.Current.CancellationToken);
        var actual = Assert.Single(result.Value);

        Assert.True(result.IsPartial);
        Assert.Equal("Test Game", actual.Name);
    }

    [Fact]
    public async Task FetchGameModsAsync_FileUnreachable_ReturnsErrorWithException()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(RenoDXModSourceService.GameModsUrl, new HttpResponseMessage(HttpStatusCode.NotFound));
        var sut = CreateService(handler);

        var result = await sut.FetchGameModsAsync(TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.IsType<HttpRequestException>(result.Exception);
    }

    [Fact]
    public async Task FetchGameModsAsync_FileNotAJsonArray_ReturnsError()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(RenoDXModSourceService.GameModsUrl, JsonResponse("{}"));
        var sut = CreateService(handler);

        var result = await sut.FetchGameModsAsync(TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task FetchGameModsAsync_OutOfRangeModStatusEntry_ReturnsPartialWithoutEntry()
    {
        const string validGameModWithUnknownStatus = """
                                                     {
                                                        "name": "Test Game",
                                                        "status": "Unknown",
                                                        "author": "Tester",
                                                        "snapshotUrl": "https://restalltests.com/renodx-game.addon64",
                                                        "snapshotUrl32": "https://restalltests.com/renodx-game.addon32",
                                                        "nexusUrl": null,
                                                        "discordUrl": null,
                                                        "discussionUrl": null,
                                                        "notes": "test game"
                                                     }
                                                     """;
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(RenoDXModSourceService.GameModsUrl,
            JsonResponse($"[{validGameModWithUnknownStatus}, {ValidGameModEntry}]"));
        var sut = CreateService(handler);

        var result = await sut.FetchGameModsAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsPartial);
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    private static RenoDXModSourceService CreateService(FakeHttpMessageHandler handler) =>
        new(NullLogger<RenoDXModSourceService>.Instance, new FakeHttpClientFactory(handler));

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
}
