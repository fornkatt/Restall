// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs;

namespace Restall.Application.Tests.DTOTests;

public class RenoDXGameModTests
{
    // 64-bit filename tests

    [Theory]
    [InlineData("renodx-thewitcher3.addon64",
        "https://oopydoopy.github.io/renodx/renodx-thewitcher3.addon64")]
    [InlineData("renodx-kingdomcome2.addon64",
        "https://clshortfuse.github.io/renodx/renodx-kingdomcome2.addon64")]
    [InlineData("renodx-007firstlight.addon64",
        "https://github.com/mqhaji/renodx/releases/download/snapshot/renodx-007firstlight.addon64")] // release url
    public void AddonFilename_HttpsUrlsToAddon64_ReturnsFilename(string expectedFilename, string snapshotUrl)
    {
        var mod = CreateMod(snapshotUrl: snapshotUrl);

        Assert.Equal(expectedFilename, mod.AddonFilename);
    }

    [Theory]
    [InlineData("https://github.com/mqhaji/renodx/releases/download/snapshot/renodx-alienisolation.addon32")]
    [InlineData("https://clshortfuse.github.io/renodx/renodx-asscreed1.addon32")]
    public void AddonFilename_Only32BitUrl_ReturnsNull(string snapshotUrl32)
    {
        var mod = CreateMod(snapshotUrl32: snapshotUrl32);

        Assert.Null(mod.AddonFilename);
    }

    [Theory]
    [InlineData("")]                                            // empty string
    [InlineData("not a url")]                                   // not a real url
    [InlineData("https://restalltest.com/")]                    // no filename
    [InlineData("http://restalltest.com/renodx-game.addon64")]  // no HTTPS
    [InlineData("/renodx-game.addon64")]                        // parses as a file
    [InlineData("https://restalltest.com/renodx-game.addon32")] // 32-bit file for a 64-bit data field
    public void AddonFilename_UnusableUrl_ReturnsNull(string snapshotUrl)
    {
        var mod = CreateMod(snapshotUrl: snapshotUrl);

        Assert.Null(mod.AddonFilename);
    }

    [Fact]
    public void AddonFilename_EscapedCharacters_ReturnsRealFilename()
    {
        var mod = CreateMod(snapshotUrl: "https://restalltest.com/renodx%20game.addon64");

        Assert.Equal("renodx game.addon64", mod.AddonFilename);
    }

    [Fact]
    public void AddonFilename_AfterWithExpression_FollowsNewUri()
    {
        var original = CreateMod(snapshotUrl: "https://restalltest.com/renodx-old.addon64");
        var copy = original with { SnapshotUrl = "https://restalltest.com/renodx-new.addon64" };

        Assert.Equal("renodx-new.addon64", copy.AddonFilename);
    }

    // 32-bit filename tests

    [Theory]
    [InlineData("renodx-asscreed1.addon32",
        "https://clshortfuse.github.io/renodx/renodx-asscreed1.addon32")]
    [InlineData("renodx-darksiders-warmastered.addon32",
        "https://oopydoopy.github.io/renodx/renodx-darksiders-warmastered.addon32")]
    [InlineData("renodx-batmanaa.addon32",
        "https://github.com/mqhaji/renodx/releases/download/snapshot/renodx-batmanaa.addon32")] // release url
    public void AddonFilename32_HttpsUrlsToAddon32_ReturnsFilename(string expectedFilename, string snapshotUrl32)
    {
        var mod = CreateMod(snapshotUrl32: snapshotUrl32);

        Assert.Equal(expectedFilename, mod.AddonFilename32);
    }

    [Theory]
    [InlineData("https://oopydoopy.github.io/renodx/renodx-thewitcher3.addon64")]
    [InlineData("https://clshortfuse.github.io/renodx/renodx-kingdomcome2.addon64")]
    public void AddonFilename32_Only64BitUrl_ReturnsNull(string snapshotUrl)
    {
        var mod = CreateMod(snapshotUrl: snapshotUrl);

        Assert.Null(mod.AddonFilename32);
    }

    [Theory]
    [InlineData("")]                                            // empty string
    [InlineData("not a url")]                                   // not a real url
    [InlineData("https://restalltest.com/")]                    // no filename
    [InlineData("http://restalltest.com/renodx-game.addon32")]  // no HTTPS
    [InlineData("/renodx-game.addon32")]                        // parses as a file
    [InlineData("https://restalltest.com/renodx-game.addon64")] // 64-bit file for a 32-bit data field
    public void AddonFilename32_UnusableUrl_ReturnsNull(string snapshotUrl32)
    {
        var mod = CreateMod(snapshotUrl32: snapshotUrl32);

        Assert.Null(mod.AddonFilename32);
    }

    [Fact]
    public void AddonFilename32_EscapedCharacters_ReturnsRealFilename()
    {
        var mod = CreateMod(snapshotUrl32: "https://restalltest.com/renodx%20game.addon32");

        Assert.Equal("renodx game.addon32", mod.AddonFilename32);
    }

    [Fact]
    public void AddonFilename32_AfterWithExpression_FollowsNewUri()
    {
        var original = CreateMod(snapshotUrl32: "https://restalltest.com/renodx-old.addon32");
        var copy = original with { SnapshotUrl32 = "https://restalltest.com/renodx-new.addon32" };

        Assert.Equal("renodx-new.addon32", copy.AddonFilename32);
    }

    private static RenoDXGameMod CreateMod(string? snapshotUrl = null, string? snapshotUrl32 = null) =>
        new("Test Game", RenoDXModStatus.Done, "Tester", snapshotUrl, snapshotUrl32,
            null, null, null, null);
}
