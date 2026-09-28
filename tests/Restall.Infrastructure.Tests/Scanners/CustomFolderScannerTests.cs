// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Domain.Entities;
using Restall.Application.Interfaces.Driven;
using Restall.Infrastructure.Scanners;

namespace Restall.Infrastructure.Tests.Scanners;

public sealed class CustomFolderScannerTests : IDisposable
{
    // Each test gets a fresh temp folder (xUnit creates a new class instance per test) and deletes it afterwards.
    private readonly string _root = Directory.CreateTempSubdirectory("restall-tests-").FullName;

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public async Task ScanAsync_ReturnsOneGamePerSubfolder()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_root, "Hades"));
        var scanner = new CustomFolderScanner(new FakeFolderProvider(_root));

        // Act
        var result = await scanner.ScanAsync();

        // Assert
        var game = Assert.Single(result.Games);
        Assert.Equal("Hades", game.Name);
        Assert.Equal(Game.Platform.Custom, game.PlatformName);
        Assert.Null(game.PlatformId);

    }

    // Handwritten stand-in so each test controls exactly which root folders are scanned.
    private sealed class FakeFolderProvider(params string[] folders) : ICustomGameFolderProvider
    {
        public IReadOnlyCollection<string> GetFolders() => folders;


    }

}



