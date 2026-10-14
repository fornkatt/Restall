// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs.RenoDXDTOs;
using System.Collections.Immutable;

namespace Restall.Application.Stores;

public sealed class RenoDXCatalog
{
    private RenoDXModDatabase _modDatabase = RenoDXModDatabase.Empty;
    private RenoDXTagInfo? _snapshot;
    private ImmutableArray<RenoDXTagInfo> _nightlies = [];

    public RenoDXModDatabase ModDatabase => _modDatabase;
    public RenoDXTagInfo? Snapshot => _snapshot;
    public ImmutableArray<RenoDXTagInfo> Nightlies => _nightlies;

    public void LoadModDatabase(RenoDXModDatabase modDatabase) => _modDatabase = modDatabase;
    public void LoadSnapshot(RenoDXTagInfo snapshot) => _snapshot = snapshot;
    public void LoadNightlies(ImmutableArray<RenoDXTagInfo> nightlies) => _nightlies = nightlies;
}
