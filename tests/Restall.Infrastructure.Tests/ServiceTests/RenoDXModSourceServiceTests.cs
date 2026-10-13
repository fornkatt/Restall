// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging.Abstractions;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Infrastructure.Services;
using Restall.Infrastructure.Tests.Fakes;
using System.Net;
using System.Text;

namespace Restall.Infrastructure.Tests.ServiceTests;

public class RenoDXModSourceServiceTests
{
    private const string GameModsUrl = "https://raw.githubusercontent.com/RankFTW/rhi-repo/main/database/RenoDXdb.json";

    private const string UnrealGenericModsUrl =
        "https://raw.githubusercontent.com/RankFTW/rhi-repo/main/database/RenoDXdb-unreal.json";

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
        handler.RespondWith(GameModsUrl, JsonResponse($"[{ValidGameModEntry}]"));
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
        handler.RespondWith(GameModsUrl, JsonResponse($"[{ValidGameModEntry}, {InvalidGameModEntry}]"));
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
        handler.RespondWith(GameModsUrl, new HttpResponseMessage(HttpStatusCode.NotFound));
        var sut = CreateService(handler);

        var result = await sut.FetchGameModsAsync(TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.IsType<HttpRequestException>(result.Exception);
    }

    [Fact]
    public async Task FetchGameModsAsync_FileNotAJsonArray_ReturnsError()
    {
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(GameModsUrl, JsonResponse("{}"));
        var sut = CreateService(handler);

        var result = await sut.FetchGameModsAsync(TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task FetchUnrealGenericModsAsync_EntryWithLiveSpellings_ReturnsSuccessWithEveryField()
    {
        const string unrealGenericModEntry = """
                                             {
                                                "Name": "Unreal Test Game",
                                                "Status": "WIP",
                                                "Method": "upgrade",
                                                "Upgrades": "`R10G10B10A2_UNORM`",
                                                "Comments": "test comment"
                                             }
                                             """;
        var handler = new FakeHttpMessageHandler();
        handler.RespondWith(UnrealGenericModsUrl, JsonResponse($"[{unrealGenericModEntry}]"));
        var sut = CreateService(handler);
        var result = await sut.FetchUnrealGenericModsAsync(TestContext.Current.CancellationToken);

        var actual = Assert.Single(result.Value);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsPartial);
        Assert.Equal(new RenoDXUnrealGenericMod("Unreal Test Game",
            RenoDXModStatus.Wip,
            RenoDXUnrealGenericMod.UnrealModMethod.Upgrade,
            "`R10G10B10A2_UNORM`",
            "test comment"), actual);
    }

    private static RenoDXModSourceService CreateService(FakeHttpMessageHandler handler) =>
        new(NullLogger<RenoDXModSourceService>.Instance, new FakeHttpClientFactory(handler));

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
}
