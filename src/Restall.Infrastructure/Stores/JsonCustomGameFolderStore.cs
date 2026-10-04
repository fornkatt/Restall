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
        _folders = LoadFolders(filePath);
    }

    private static List<string> LoadFolders(string filePath)
    {
        if (!File.Exists(filePath)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(filePath)) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public IReadOnlyCollection<string> GetFolders() => _folders;

    public void AddFolder(string path)
    {
        if (_folders.Contains(path)) return;
        _folders.Add(path);
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        File.WriteAllText(_filePath, JsonSerializer.Serialize(_folders));
    }
}
