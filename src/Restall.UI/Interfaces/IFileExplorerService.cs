// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Threading.Tasks;

namespace Restall.UI.Interfaces;

public interface IFileExplorerService
{
    Task OpenFolderAsync(string folder);
}
