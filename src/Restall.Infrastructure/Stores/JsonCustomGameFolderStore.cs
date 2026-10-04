// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Interfaces.Driven;
using System.Text.Json;

namespace Restall.Infrastructure.Stores;

internal sealed class JsonCustomGameFolderStore : ICustomGameFolderStore
{
    private readonly string _filePath;
    private readonly List<string> _folders;

    public JsonCustomGameFolderStore(string filePath)
    {
        _filePath = filePath;
        _folders = File.Exists(filePath)
            ? JsonSerializer.Deserialize<List<string>>(File.ReadAllText(filePath)) ?? []
            : [];
    }

    public IReadOnlyCollection<string> GetFolders() => _folders;

    public void AddFolder(string path)
    {
        _folders.Add(path);
        File.WriteAllText(_filePath, JsonSerializer.Serialize(_folders));
    }
}
