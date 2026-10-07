// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;
using System.Collections.Immutable;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXDownloadOptions(
    string Filename,
    Uri? DirectUrl,
    RenoDXTagInfo? Snapshot,
    ImmutableArray<RenoDXTagInfo> Nightlies)
{
    public ImmutableArray<RenoDX.Branch> Branches
    {
        get
        {
            var branches = ImmutableArray.CreateBuilder<RenoDX.Branch>(3);

            if (Snapshot is not null)
                branches.Add(RenoDX.Branch.Snapshot);
            if (!Nightlies.IsEmpty)
                branches.Add(RenoDX.Branch.Nightly);
            if (DirectUrl is not null)
                branches.Add(RenoDX.Branch.Direct);

            return branches.DrainToImmutable();
        }
    }
}
