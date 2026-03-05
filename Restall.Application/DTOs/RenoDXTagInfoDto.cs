using Restall.Domain.Entities;

namespace Restall.Application.DTOs;

public record RenoDXTagInfoDto(
    DateOnly Date,
    RenoDX.Branch Branch,
    List<string>? CommitNotes = null
)
{
    public string Version => Branch == RenoDX.Branch.Nightly
        ? $"nightly-{Date:yyyyMMdd}"
        : $"{Date:yyyyMMdd}";
}