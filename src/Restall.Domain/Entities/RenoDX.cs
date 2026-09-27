// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Common.Enums;

namespace Restall.Domain.Entities;

public sealed class RenoDX
{
    public enum Branch { Unknown, Wiki, Snapshot, Nightly, Discord, Nexus }

    public string? SelectedName { get; set; }
    public string? OriginalName { get; set; }
    public Branch BranchName { get; set; } = Branch.Unknown;
    public Architecture Arch { get; set; } = Architecture.X64;
    public string? Version { get; set; }

    public bool IsUpdateCheckSupported =>
        OriginalName is null ||
        (!OriginalName.StartsWith("renodx-unityengine", StringComparison.OrdinalIgnoreCase) &&
         !OriginalName.StartsWith("renodx-ue-extended", StringComparison.OrdinalIgnoreCase));
}
