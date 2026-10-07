// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Restall.Application.DTOs.RenoDXDTOs;

public sealed record RenoDXEntry(
    bool IsSupported,
    // What the user can get
    RenoDXDownloadOptions? DownloadOptions,
    RenoDXModLink? ManualSource,
    FrozenDictionary<RenoDX.Branch, UpdateCheck> BranchVersions,
    // Database information
    string? ListedName,
    string? StatusText,
    string? Author,
    string? DatabaseNotes,
    string? UnrealMethodText,
    ImmutableArray<string> Upgrades,
    ImmutableArray<RenoDXModLink> Links,
    // Restall additions
    ImmutableArray<string> Notices,
    RenoDXGenericAddonInfo? GenericAddonInfo);
