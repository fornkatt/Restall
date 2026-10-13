// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Restall.Application.DTOs.RenoDXDTOs;

/// <summary>
/// This record holds a whole RenoDX entry, assembled for one game.
/// <br/><br/>
/// They are grouped by what the user can get, database information and Restall additions.
/// </summary>
/// <param name="IsSupported">Whether the game supports RenoDX.</param>
/// <param name="DownloadOptions">What download options this RenoDX mod has for a specific game.</param>
/// <param name="ManualSource">The manual source where to get the mod (e.g. Nexus).</param>
/// <param name="BranchVersions">What versions are available for certain branches.</param>
/// <param name="BranchReleaseNotes">Release notes per branch, per version.</param>
/// <param name="HeadingText">The heading text (Listed as...).</param>
/// <param name="StatusText">The status text shown in the app (derived from Done, Wip).</param>
/// <param name="IsDone">Easy to access bool for the mod's Done status.</param>
/// <param name="IsWorkInProgress">Easy to access bool for the mod's Wip status.</param>
/// <param name="Author">Name of the mod's author.</param>
/// <param name="DatabaseNotes">Notes/comments fetched directly from the database as-is.</param>
/// <param name="UnrealMethodText">What method the listed Unreal mod uses (Native, Upgrade, Ini).</param>
/// <param name="Upgrades">What upgrades need to be set, used for generic mods. Fetched from the database.</param>
/// <param name="Links">All links associated with the mod.</param>
/// <param name="Notices">Notices the app needs to convey to the user through the UI.</param>
/// <param name="GenericAddonInfo">Holds all information about the various generic mods available.</param>
public sealed record RenoDXEntry(
    bool IsSupported,
    // What the user can get
    RenoDXDownloadOptions? DownloadOptions,
    RenoDXModLink? ManualSource,
    FrozenDictionary<RenoDX.Branch, UpdateAvailability> BranchVersions,
    FrozenDictionary<RenoDX.Branch, ImmutableArray<RenoDXReleaseNotes>> BranchReleaseNotes,
    // Database information
    string? HeadingText,
    string? StatusText,
    bool IsDone,
    bool IsWorkInProgress,
    string? Author,
    string? DatabaseNotes,
    string? UnrealMethodText,
    ImmutableArray<string> Upgrades,
    ImmutableArray<RenoDXModLink> Links,
    // Restall additions
    ImmutableArray<string> Notices,
    RenoDXGenericAddonInfo? GenericAddonInfo);
