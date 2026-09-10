// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Text.RegularExpressions;

namespace Restall.Application.Helpers;

public static partial class EmojiShortcodeHelper
{
    private static readonly Dictionary<string, string> s_shortcodeToEmoji = new(StringComparer.OrdinalIgnoreCase)
    {
        ["white_check_mark"] = "✅",
        ["construction"] = "🚧",
        ["warning"] = "⚠️",
        ["exclamation"] = "❗",
        ["question"] = "❓",
        ["sunny"] = "☀️",
        ["no_entry"] = "🚫",
        ["boom"] = "💥",
        ["unlock"] = "🔓",
        ["lock"] = "🔒",
        ["zap"] = "⚡",
        ["bulb"] = "💡",
        ["fire"] = "🔥",
        ["rocket"] = "🚀",
        ["information_source"] = "ℹ️"
    };

    [GeneratedRegex(@":([a-zA-Z0-9_+\-]+):")]
    private static partial Regex ShortcodeRegex();

    public static string Convert(string text) =>
        ShortcodeRegex().Replace(text, m =>
            s_shortcodeToEmoji.TryGetValue(m.Groups[1].Value, out var emoji) ? emoji : m.Value);
}
