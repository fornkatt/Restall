// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.UI.Interfaces;

namespace Restall.UI.Tests.Fakes;

public class FakeFileExplorerService : IFileExplorerService
{
    private readonly List<string> _openedFolders = [];

    public IReadOnlyList<string> OpenedFolders => _openedFolders;

    public Task OpenFolderAsync(string folder)
    {
        _openedFolders.Add(folder);

        return Task.CompletedTask;
    }
}
