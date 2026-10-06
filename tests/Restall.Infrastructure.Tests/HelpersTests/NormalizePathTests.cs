// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Helpers;
using Restall.Infrastructure.Tests.Common;

namespace Restall.Infrastructure.Tests.HelpersTests;

public class NormalizePathTests
{
    public static TheoryData<string> InvalidPaths() =>
    [
        "",
        null!
    ];

    [Theory]
    [MemberData(nameof(InvalidPaths))]
    public void NormalizePath_NullOrEmpty_ReturnsNull(string input)
    {
        var expected = GameScanHelper.NormalizePath(input);
        Assert.Null(expected);
    }

    [Theory(Skip = "Windows pathing",
        SkipUnless = nameof(Shared.IsWindows),
        SkipType = typeof(Shared))]
    [InlineData("C:/Games/Heroic/Alan Wake", @"C:\Games\Heroic\Alan Wake")]
    public void NormalizePath_WindowsForwardSlashes_ReturnsBackSlashes(string input, string expected)
    {
        // Arrange
        var actual = GameScanHelper.NormalizePath(input);

        //Assert
        Assert.Equal(expected, actual);

    }

    [Theory(Skip = "Windows pathing",
        SkipUnless = nameof(Shared.IsWindows),
        SkipType = typeof(Shared))]
    [InlineData("C:\\Epic Games\\LEGOStarWarsTSS\\", @"C:\Epic Games\LEGOStarWarsTSS")]
    public void NormalizePath_WindowsTrailingSeparator_ReturnsUnchanged(string input, string expected)
    {
        // Act
        var actual = GameScanHelper.NormalizePath(input);

        //Assert
        Assert.Equal(expected, actual);
    }

    [Fact(Skip = "Linux pathing",
        SkipUnless = nameof(Shared.IsLinux),
        SkipType = typeof(Shared))]
    public void NormalizePath_LinuxTrailingSeparator_ReturnsUnchanged()
    {
        // Act
        var actual = GameScanHelper.NormalizePath("/home/user/Games/Heroic/Alan Wake 2/");

        //Assert
        Assert.Equal("/home/user/Games/Heroic/Alan Wake 2", actual);
    }




}
