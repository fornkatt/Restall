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