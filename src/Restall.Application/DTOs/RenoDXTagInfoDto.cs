// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;

namespace Restall.Application.DTOs;

public record RenoDXTagInfoDto(
    DateOnly Date,
    RenoDX.Branch Branch,
    List<string>? CommitNotes = null)
{
    public string Version => $"{Date:yyyyMMdd}";
}
