// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging.Abstractions;
using Restall.Domain.Entities;
using Restall.Application.Interfaces.Driven;
using Restall.Infrastructure.Scanners;

namespace Restall.Infrastructure.Tests.Scanners;

public sealed class CustomFolderScannerTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("restall-tests-").FullName;

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public async Task ScanAsync_LookForGameInSubFolder_ReturnsExpectedResult()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_root, "Hades"));
        var sut = new CustomFolderScanner(new FakeFolderProvider(_root), NullLogger<CustomFolderScanner>.Instance);

        // Act
        var result = await sut.ScanAsync();

        // Assert
        var game = Assert.Single(result.Games);
        Assert.Equal("Hades", game.Name);
        Assert.Equal(Game.Platform.Custom, game.PlatformName);
        Assert.Null(game.PlatformId);
    }

    [Fact]
    public async Task ScanAsync_ScanCustomFolder_ReturnsGameFoldersAndSkipsNonGameFolders()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_root, "Hades"));
        Directory.CreateDirectory(Path.Combine(_root, "_CommonRedist"));
        var sut = new CustomFolderScanner(new FakeFolderProvider(_root), NullLogger<CustomFolderScanner>.Instance);

        // Act
        var result = await sut.ScanAsync();

        // Assert
        var game = Assert.Single(result.Games);
        Assert.Equal("Hades", game.Name);
    }

    [Fact]
    public async Task ScanAsync_FolderDoesNotExist_ReturnsNoGames()
    {
        // Arrange
        var missingFolder = Path.Combine(_root, "DoesNotExist");
        var sut = new CustomFolderScanner(new FakeFolderProvider(missingFolder), NullLogger<CustomFolderScanner>.Instance);

        // Act
        var result = await sut.ScanAsync();

        // Assert
        Assert.Empty(result.Games);
    }

    [Fact]
    public async Task ScanAsync_OneFolderIsMissing_ReturnsGamesFromOtherFolders()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_root, "Hades"));
        var missingFolder = Path.Combine(_root, "MissingFolder");
        var sut = new CustomFolderScanner(new FakeFolderProvider(missingFolder, _root), NullLogger<CustomFolderScanner>.Instance);

        // Act
        var result = await sut.ScanAsync();

        // Assert
        var game = Assert.Single(result.Games);
        Assert.Equal("Hades", game.Name);
    }

    private sealed class FakeFolderProvider(params string[] folders) : ICustomGameFolderProvider
    {
        public IReadOnlyCollection<string> GetFolders() => folders;
    }


}



