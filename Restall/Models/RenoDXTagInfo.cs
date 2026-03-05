using System;
using System.Collections.Generic;

namespace Restall.Models;

public record RenoDXTagInfo(
    DateOnly Date,
    RenoDX.Branch Branch,
    List<string>? CommitNotes = null
)
{
    public string Version => Branch == RenoDX.Branch.Nightly
        ? $"nightly-{Date:yyyyMMdd}"
        : $"{Date:yyyyMMdd}";
}