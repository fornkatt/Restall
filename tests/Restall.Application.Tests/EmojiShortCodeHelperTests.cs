// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Helpers;

namespace Restall.Application.Tests;

//TODO: REMOVE THIS SCRIPT, IT IS SIMPLY HERE TO IMPLEMENT CI.YML
public class EmojiShortCodeHelperTests
{
    [Theory]
    [InlineData(":rocket:", "🚀")]
    public void Convert_EmojiCode_ReturnsEmoji(string input, string expected)
    {
        var actual = EmojiShortcodeHelper.Convert(input);
        Assert.Equal(expected, actual);
    }
}
