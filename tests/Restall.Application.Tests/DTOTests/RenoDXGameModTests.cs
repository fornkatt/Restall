// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs;

namespace Restall.Application.Tests.DTOTests;

public class RenoDXGameModTests
{
    public static TheoryData<string, string> ValidHttpsUrls(string extension) => new()
    {
        { $"renodx-game{extension}", $"https://clshortfuse.github.io/renodx/renodx-game{extension}" },
        { $"renodx-game{extension}",
            $"https://github.com/mqhaji/renodx/releases/download/snapshot/renodx-game{extension}" },    // release url
        { $"renodx-game{extension}", $"https://restalltest.com/renodx-game{extension}" }
    };

    public static TheoryData<string> UnusableUrls(string extension, string otherExtension) =>
    [
        "",                                                     // empty string
        "not a url",                                            // not a real url
        "https://restalltest.com/",                             // no filename
        $"http://restalltest.com/renodx-game{extension}",       // no HTTPS
        $"/renodx-game{extension}",                             // parses as a file
        $"https://restalltest.com/renodx-game{otherExtension}"  // the other bitness' file
    ];

    // 64-bit filename tests

    [Theory]
    [MemberData(nameof(ValidHttpsUrls), ".addon64")]
    public void AddonFilename_ValidHttpsUrlsToAddon64_ReturnsFilename(string expectedFilename, string snapshotUrl)
    {
        var mod = CreateMod(snapshotUrl: snapshotUrl);

        Assert.Equal(expectedFilename, mod.AddonFilename);
    }

    [Fact]
    public void AddonFilename_Only32BitUrl_ReturnsNull()
    {
        var mod = CreateMod(snapshotUrl32: "https://restalltest.com/renodx-game.addon32");

        Assert.Null(mod.AddonFilename);
    }

    [Theory]
    [MemberData(nameof(UnusableUrls), ".addon64", ".addon32")]
    public void AddonFilename_UnusableUrl_DoesNotFallBackTo32BitFile(string snapshotUrl)
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

    [Fact]
    public void AddonFilename_IgnoresCase_ReturnsFilenameWithPreservedCase()
    {
        var mod = CreateMod(snapshotUrl: "https://restalltest.com/Renodx-New.ADDoN64");

        Assert.Equal("Renodx-New.ADDoN64", mod.AddonFilename);
    }

    // 32-bit filename tests

    [Theory]
    [MemberData(nameof(ValidHttpsUrls), ".addon32")]
    public void AddonFilename32_ValidHttpsUrlsToAddon32_ReturnsFilename(string expectedFilename, string snapshotUrl32)
    {
        var mod = CreateMod(snapshotUrl32: snapshotUrl32);

        Assert.Equal(expectedFilename, mod.AddonFilename32);
    }

    [Fact]
    public void AddonFilename32_Only64BitUrl_DoesNotFallBackTo64BitFile()
    {
        var mod = CreateMod(snapshotUrl: "https://restalltest.com/renodx-game.addon64");

        Assert.Null(mod.AddonFilename32);
    }

    [Theory]
    [MemberData(nameof(UnusableUrls), ".addon32", ".addon64")]
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

    [Fact]
    public void AddonFilename32_IgnoresCase_ReturnsFilenameWithPreservedCase()
    {
        var mod = CreateMod(snapshotUrl32: "https://restalltest.com/Renodx-New.ADDoN32");

        Assert.Equal("Renodx-New.ADDoN32", mod.AddonFilename32);
    }

    private static RenoDXGameMod CreateMod(string? snapshotUrl = null, string? snapshotUrl32 = null) =>
        new("Test Game", RenoDXModStatus.Done, "Tester", snapshotUrl, snapshotUrl32,
            null, null, null, null);
}
