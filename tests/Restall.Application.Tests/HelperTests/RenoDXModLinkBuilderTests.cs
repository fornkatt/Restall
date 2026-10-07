// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Helpers;

namespace Restall.Application.Tests.HelperTests;

public class RenoDXModLinkBuilderTests
{
    [Theory]
    [InlineData("https://www.nexusmods.com/testgame/mods/1", "Nexus Mods")]
    [InlineData("https://gamebanana.com/mods/1", "GameBanana")]
    public void CreateModPageLink_KnownModSite_ReturnsSiteNameLabel(string url, string expectedLabel)
    {
        var actual = RenoDXModLinkBuilder.CreateModPageLink(url);

        Assert.NotNull(actual);
        Assert.Equal(expectedLabel, actual.Label);
        Assert.Equal(new Uri(url), actual.Url);
    }

    [Fact]
    public void CreateModPageLink_UnknownSite_ReturnsHostWithoutWww()
    {
        var actual = RenoDXModLinkBuilder.CreateModPageLink("https://www.restalltests.com/mods/1");

        Assert.NotNull(actual);
        Assert.Equal("restalltests.com", actual.Label);
    }

    [Theory]
    [InlineData("http://nexusmods.com/testgame/mods/1")]
    [InlineData("not a link")]
    [InlineData(null)]
    public void CreateModPageLink_NotHttpsLink_ReturnsNothing(string? url)
    {
        var actual = RenoDXModLinkBuilder.CreateModPageLink(url);

        Assert.Null(actual);
    }

    [Fact]
    public void CreateDiscordLink_HttpsLink_ReturnsDiscordLabel()
    {
        var actual = RenoDXModLinkBuilder.CreateDiscordLink("https://discord.com/channels/1/2");

        Assert.NotNull(actual);
        Assert.Equal("Discord", actual.Label);
    }

    [Fact]
    public void CreateDiscordLink_HttpLink_ReturnsNothing()
    {
        var actual = RenoDXModLinkBuilder.CreateDiscordLink("http://discord.com/channels/1/2");

        Assert.Null(actual);
    }

    [Fact]
    public void CreateGitHubDiscussionLink_HttpsLink_ReturnsGitHubDiscussionLabel()
    {
        var actual = RenoDXModLinkBuilder.CreateGitHubDiscussionLink(
            "https://github.com/restalltests/test/discussions/1");

        Assert.NotNull(actual);
        Assert.Equal("GitHub discussion", actual.Label);
    }

    [Fact]
    public void CreateGitHubDiscussionLink_HttpLink_ReturnsNothing()
    {
        var actual = RenoDXModLinkBuilder.CreateGitHubDiscussionLink(
            "http://github.com/restalltests/test/discussions/1");

        Assert.Null(actual);
    }
}
