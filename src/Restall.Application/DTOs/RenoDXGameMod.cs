// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;

namespace Restall.Application.DTOs;

public record RenoDXGameMod(
    string Name,
    RenoDXModStatus Status,
    string? Author,
    string? SnapshotUrl,
    string? SnapshotUrl32,
    string? NexusUrl,
    string? DiscordUrl,
    string? DiscussionUrl,
    string? Notes);
