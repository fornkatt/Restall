using Restall.Domain.Entities;

namespace Restall.Application.DTOs;

public record RenoDXTagInfoDto(
    DateOnly Date,
    RenoDX.Branch Branch,
    List<string>? CommitNotes = null
)
{
    public string Version => $"{Date:yyyyMMdd}";
}