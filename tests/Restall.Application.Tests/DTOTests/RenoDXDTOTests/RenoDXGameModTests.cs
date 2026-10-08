// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Domain.Common.Enums;

namespace Restall.Application.Tests.DTOTests.RenoDXDTOTests;

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
    public void GetAddonFilename_ValidHttpsUrls64_ReturnsFilename(string expectedFilename, string snapshotUrl)
    {
        var mod = CreateMod(snapshotUrl: snapshotUrl);

        var actual = mod.GetAddonFilename(Architecture.X64);

        Assert.Equal(expectedFilename, actual);
    }

    [Fact]
    public void GetAddonFilename_OnlyUrl32_DoesNotFallBackTo32BitFile()
    {
        var mod = CreateMod(snapshotUrl32: "https://restalltest.com/renodx-game.addon32");

        var actual = mod.GetAddonFilename(Architecture.X64);

        Assert.Null(actual);
    }

    [Theory]
    [MemberData(nameof(UnusableUrls), ".addon64", ".addon32")]
    public void GetAddonFilename_UnusableUrls64_ReturnsNull(string snapshotUrl)
    {
        var mod = CreateMod(snapshotUrl: snapshotUrl);

        var actual = mod.GetAddonFilename(Architecture.X64);

        Assert.Null(actual);
    }

    [Fact]
    public void GetAddonFilename_EscapedCharacters64_ReturnsRealFilename()
    {
        var mod = CreateMod(snapshotUrl: "https://restalltest.com/renodx%20game.addon64");

        var actual = mod.GetAddonFilename(Architecture.X64);

        Assert.Equal("renodx game.addon64", actual);
    }

    [Fact]
    public void GetAddonFilename_AfterWithExpression64_FollowsNewUri()
    {
        var original = CreateMod(snapshotUrl: "https://restalltest.com/renodx-old.addon64");
        var copy = original with { SnapshotUrl = "https://restalltest.com/renodx-new.addon64" };

        var actual = copy.GetAddonFilename(Architecture.X64);

        Assert.Equal("renodx-new.addon64", actual);
    }

    [Fact]
    public void GetAddonFilename_IgnoresCase64_ReturnsFilenameWithPreservedCase()
    {
        var mod = CreateMod(snapshotUrl: "https://restalltest.com/Renodx-New.ADDoN64");

        var actual = mod.GetAddonFilename(Architecture.X64);

        Assert.Equal("Renodx-New.ADDoN64", actual);
    }

    // 32-bit filename tests

    [Theory]
    [MemberData(nameof(ValidHttpsUrls), ".addon32")]
    public void GetAddonFilename_ValidHttpsUrls32_ReturnsFilename(string expectedFilename, string snapshotUrl32)
    {
        var mod = CreateMod(snapshotUrl32: snapshotUrl32);

        var actual = mod.GetAddonFilename(Architecture.X32);

        Assert.Equal(expectedFilename, actual);
    }

    [Fact]
    public void GetAddonFilename_OnlyUrl64_DoesNotFallBackTo64BitFile()
    {
        var mod = CreateMod(snapshotUrl: "https://restalltest.com/renodx-game.addon64");

        var actual = mod.GetAddonFilename(Architecture.X32);

        Assert.Null(actual);
    }

    [Theory]
    [MemberData(nameof(UnusableUrls), ".addon32", ".addon64")]
    public void GetAddonFilename_UnusableUrls32_ReturnsNull(string snapshotUrl32)
    {
        var mod = CreateMod(snapshotUrl32: snapshotUrl32);

        var actual = mod.GetAddonFilename(Architecture.X32);

        Assert.Null(actual);
    }

    [Fact]
    public void GetAddonFilename_EscapedCharacters32_ReturnsRealFilename()
    {
        var mod = CreateMod(snapshotUrl32: "https://restalltest.com/renodx%20game.addon32");

        var actual = mod.GetAddonFilename(Architecture.X32);

        Assert.Equal("renodx game.addon32", actual);
    }

    [Fact]
    public void GetAddonFilename_AfterWithExpression32_FollowsNewUri()
    {
        var original = CreateMod(snapshotUrl32: "https://restalltest.com/renodx-old.addon32");
        var copy = original with { SnapshotUrl32 = "https://restalltest.com/renodx-new.addon32" };

        var actual = copy.GetAddonFilename(Architecture.X32);

        Assert.Equal("renodx-new.addon32", actual);
    }

    [Fact]
    public void GetAddonFilename_IgnoresCase32_ReturnsFilenameWithPreservedCase()
    {
        var mod = CreateMod(snapshotUrl32: "https://restalltest.com/Renodx-New.ADDoN32");

        var actual = mod.GetAddonFilename(Architecture.X32);

        Assert.Equal("Renodx-New.ADDoN32", actual);
    }

    // Download URL tests

    [Fact]
    public void GetDownloadUrl_64BitAddonFilename_Returns64BitUrl()
    {
        var mod = CreateMod("https://restalltests.com/renodx-game.addon64",
            "https://restalltests.com/renodx-game.addon32");

        Assert.Equal(new Uri("https://restalltests.com/renodx-game.addon64"),
            mod.GetDownloadUrl("renodx-game.addon64"));
    }

    [Fact]
    public void GetDownloadUrl_32BitAddonFilename_Returns32BitUrl()
    {
        var mod = CreateMod("https://restalltests.com/renodx-game.addon64",
            "https://restalltests.com/renodx-game.addon32");

        Assert.Equal(new Uri("https://restalltests.com/renodx-game.addon32"),
            mod.GetDownloadUrl("renodx-game.addon32"));
    }

    [Fact]
    public void GetDownloadUrl_AddonFilenameWithOtherCasing_ReturnsUrl()
    {
        var mod = CreateMod("https://restalltests.com/renodx-game.addon64");

        Assert.Equal(new Uri("https://restalltests.com/renodx-game.addon64"),
            mod.GetDownloadUrl("RenoDX-Game.addon64"));
    }

    [Fact]
    public void GetDownloadUrl_OtherModsAddonFilename_ReturnsNull()
    {
        var mod = CreateMod("https://restalltests.com/renodx-game.addon64");

        Assert.Null(mod.GetDownloadUrl("renodx-othergame.addon64"));
    }

    private static RenoDXGameMod CreateMod(string? snapshotUrl = null, string? snapshotUrl32 = null) =>
        new("Test Game", RenoDXModStatus.Done, "Tester", snapshotUrl, snapshotUrl32,
            null, null, null, null);
}
