// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Infrastructure.Stores;
using System.ComponentModel.Design.Serialization;

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

    [Fact]
    public void AddFolder_IgnoresDuplicates()
    {
        //Arrange
        var store = new JsonCustomGameFolderStore(FilePath);

        //Act
        store.AddFolder("/Games");
        store.AddFolder("/Games");

        //Assert
        Assert.Single(store.GetFolders());
    }

    [Fact]
    public void Constructor_StartsEmpty_WhenFileIsCorrupt()
    {
        //Arrange
        File.WriteAllText(FilePath, "this is not json");

        //Act
        var store = new JsonCustomGameFolderStore(FilePath);

        //Assert
        Assert.Empty(store.GetFolders());
    }

    [Fact]
    public void AddFolder_CreatesDirectory_WhenItDoesNotExist()
    {
        //Arrange
        var filePath = Path.Combine(_tempDir, "NewFolder", "custom-game-folders.json");
        var store = new JsonCustomGameFolderStore(filePath);

        //Act
        store.AddFolder("/Games");

        //Assert
        Assert.True(File.Exists(filePath));
    }
}
