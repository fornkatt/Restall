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
    public void AddFolder_StoreIsReloaded_ReturnsSavedFolder()
    {
        // Arrange
        var sut = new JsonCustomGameFolderStore(FilePath);

        // Act
        sut.AddFolder("/Games");
        var reloaded = new JsonCustomGameFolderStore(FilePath);

        // Assert
        var folder = Assert.Single(reloaded.GetFolders());
        Assert.Equal("/Games", folder);
    }

    [Fact]
    public void AddFolder_SameFolderIsAddedTwice_StoresItOnce()
    {
        // Arrange
        var sut = new JsonCustomGameFolderStore(FilePath);

        // Act
        sut.AddFolder("/Games");
        sut.AddFolder("/Games");

        // Assert
        Assert.Single(sut.GetFolders());
    }

    [Fact]
    public void Constructor_FileIsCorrupt_StartsWithNoFolders()
    {
        // Arrange
        File.WriteAllText(FilePath, "this is not json");

        // Act
        var sut = new JsonCustomGameFolderStore(FilePath);

        // Assert
        Assert.Empty(sut.GetFolders());
    }

    [Fact]
    public void AddFolder_DirectoryDoesNotExist_CreatesFile()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "NewFolder", "custom-game-folders.json");
        var sut = new JsonCustomGameFolderStore(filePath);

        // Act
        sut.AddFolder("/Games");

        // Assert
        Assert.True(File.Exists(filePath));
    }
}
