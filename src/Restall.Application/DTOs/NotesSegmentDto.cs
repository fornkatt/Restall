// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.DTOs;

public enum NotesSegmentKind
{
    PlainText,
    Code
}

public record NotesSegmentDto(
    NotesSegmentKind Kind,
    string Text
)
{
    public bool IsPlainText => Kind == NotesSegmentKind.PlainText;
    public bool IsCode => Kind == NotesSegmentKind.Code;
}