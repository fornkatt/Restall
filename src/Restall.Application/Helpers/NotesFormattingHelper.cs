// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Restall.Application.DTOs;

namespace Restall.Application.Helpers;

public static partial class NotesFormattingHelper
{
    [GeneratedRegex(@"^```[^\n]*\n(?<code>.*?)\n```[ \t]*$",
        RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex CodeFenceRegex();

    public static ImmutableArray<NotesSegmentDto> Segment(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        var segments = new List<NotesSegmentDto>();
        var lastIndex = 0;

        foreach (Match match in CodeFenceRegex().Matches(raw))
        {
            AddPlainText(segments, raw, lastIndex, match.Index);

            var code = match.Groups["code"].Value.TrimEnd();
            if (!string.IsNullOrWhiteSpace(code))
                segments.Add(new NotesSegmentDto(NotesSegmentKind.Code, code));

            lastIndex = match.Index + match.Length;
        }

        AddPlainText(segments, raw, lastIndex, raw.Length);

        return [.. segments];
    }

    private static void AddPlainText(List<NotesSegmentDto> segments, string raw, int start, int end)
    {
        if (end <= start)
            return;

        var plain = raw[start..end].Trim();

        if (!string.IsNullOrWhiteSpace(plain))
            segments.Add(new NotesSegmentDto(NotesSegmentKind.PlainText, plain));
    }
}
