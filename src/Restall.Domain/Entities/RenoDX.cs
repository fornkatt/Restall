// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Common.Enums;
using System.Globalization;

namespace Restall.Domain.Entities;

public sealed class RenoDX
{
    public enum Branch { Unknown, Direct, Snapshot, Nightly, Discord, Nexus }

    public const string VersionFormat = "yyyyMMdd";
    public const string AddonExtension = ".addon64";
    public const string AddonExtension32 = ".addon32";
    public const string OriginalFilenameStart = "renodx-";

    public string? SelectedName { get; set; }
    public string? OriginalName { get; set; }
    public Branch BranchName { get; set; } = Branch.Unknown;
    public Architecture Arch { get; set; } = Architecture.X64;
    public string? Version { get; set; }

    public DateOnly? BuildDate =>
        DateOnly.TryParseExact(Version, VersionFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
            out var buildDate)
            ? buildDate
            : null;

    public bool IsUpdateCheckSupported =>
        OriginalName is null ||
        (!OriginalName.StartsWith("renodx-unityengine", StringComparison.OrdinalIgnoreCase) &&
         !OriginalName.StartsWith("renodx-ue-extended", StringComparison.OrdinalIgnoreCase));
}
