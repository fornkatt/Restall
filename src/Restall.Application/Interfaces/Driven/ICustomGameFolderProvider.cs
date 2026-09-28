// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.Application.Interfaces.Driven;

/// <summary>
/// Supplies the user-selected root folders that <c>CustomFolderScanner</c> searches for games.
/// Each direct subfolder of a root gets treated as one game.
/// </summary>
public interface ICustomGameFolderProvider
{
    IReadOnlyCollection<string> GetFolders();
}
