// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Stores;
namespace Restall.Infrastructure.Tests.Stores;
public sealed class JsonCustomGameFolderStoreTests : IDisposable
{
    private readonly string _tempDir = Directory.CreateTempSubdirectory("restall-tests-").FullName;
    private string FilePath => Path.Combine(_tempDir, "custom-game-folders.json");
    public void Dispose() => Directory.Delete(_tempDir, recursive: true);
    [Fact]
    public void AddFolder_IsRemembered_AfterRestart()
    {
        // Arrange
        var store = new JsonCustomGameFolderStore(FilePath);

        // Act
        store.AddFolder("/Games");
        var reloaded = new JsonCustomGameFolderStore(FilePath);

        // Assert
        var folder = Assert.Single(reloaded.GetFolders());
        Assert.Equal("/Games", folder);
    }
}
