using Restall.Domain.Entities;
using Restall.Infrastructure.Services;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Infrastructure;

public sealed class ModInstallServiceTests
{
    // Verifies that installing ReShade copies the source file and updates the game state.
    [Fact]
    public async Task InstallModAsync_WhenInstallingReShade_CopiesFileAndUpdatesGameState()
    {
        using var temp = new TempDirectory();
        var gameDir = temp.CreateDirectory("game");
        var sourcePath = temp.CreateFile(Path.Combine("cache", "ReShade64.dll"), "reshade-content");
        var game = CreateGame(gameDir);
        var reShade = new ReShade
        {
            SelectedFilename = "dxgi.dll",
            Version = "6.5.0"
        };
        var sut = CreateSut();

        var result = await sut.InstallModAsync(game, reShade, sourcePath);

        Assert.True(result.IsSuccess);
        Assert.Same(game, result.UpdatedGame);
        Assert.Same(reShade, game.ReShade);
        Assert.Equal("reshade-content", File.ReadAllText(Path.Combine(gameDir, "dxgi.dll")));
    }

    // Verifies that replacing ReShade removes the old selected file and copies the new one.
    [Fact]
    public async Task InstallModAsync_WhenReplacingExistingReShade_RemovesOldFileAndCopiesNewFile()
    {
        using var temp = new TempDirectory();
        var gameDir = temp.CreateDirectory("game");
        var sourcePath = temp.CreateFile(Path.Combine("cache", "ReShade64.dll"), "new-content");
        var oldPath = temp.CreateFile(Path.Combine("game", "old.dll"), "old-content");
        var game = CreateGame(gameDir);
        game.ReShade = new ReShade { SelectedFilename = "old.dll", Version = "6.4.0" };
        var reShade = new ReShade
        {
            SelectedFilename = "dxgi.dll",
            Version = "6.5.0"
        };
        var sut = CreateSut();

        var result = await sut.InstallModAsync(game, reShade, sourcePath);

        Assert.True(result.IsSuccess);
        Assert.False(File.Exists(oldPath));
        Assert.Equal("new-content", File.ReadAllText(Path.Combine(gameDir, "dxgi.dll")));
        Assert.Same(reShade, game.ReShade);
    }

    // Verifies that a missing ReShade source file returns a failure without changing game state.
    [Fact]
    public async Task InstallModAsync_WhenReShadeSourceIsMissing_ReturnsFailure()
    {
        using var temp = new TempDirectory();
        var gameDir = temp.CreateDirectory("game");
        var game = CreateGame(gameDir);
        var reShade = new ReShade { SelectedFilename = "dxgi.dll" };
        var sut = CreateSut();

        var result = await sut.InstallModAsync(game, reShade, temp.GetPath("missing", "ReShade64.dll"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Install failed. Disk may be full or the game folder was moved.", result.Message);
        Assert.Null(game.ReShade);
        Assert.False(File.Exists(Path.Combine(gameDir, "dxgi.dll")));
    }

    // Verifies that installing RenoDX copies the addon file and updates the game state.
    [Fact]
    public async Task InstallModAsync_WhenInstallingRenoDX_CopiesFileAndUpdatesGameState()
    {
        using var temp = new TempDirectory();
        var gameDir = temp.CreateDirectory("game");
        var sourcePath = temp.CreateFile(Path.Combine("cache", "renodx-game.addon64"), "renodx-content");
        var game = CreateGame(gameDir);
        var renoDX = new RenoDX
        {
            OriginalName = "renodx-game.addon64",
            SelectedName = "renodx-game.addon64",
            Version = "20240101"
        };
        var sut = CreateSut();

        var result = await sut.InstallModAsync(game, renoDX, sourcePath);

        Assert.True(result.IsSuccess);
        Assert.Same(game, result.UpdatedGame);
        Assert.Same(renoDX, game.RenoDX);
        Assert.Equal("renodx-content", File.ReadAllText(Path.Combine(gameDir, "renodx-game.addon64")));
    }

    // Verifies that RenoDX installs under the selected name when original and selected names differ.
    [Fact]
    public async Task InstallModAsync_WhenRenoDXOriginalNameDiffersFromSelectedName_CopiesSelectedFile()
    {
        using var temp = new TempDirectory();
        var gameDir = temp.CreateDirectory("game");
        var sourcePath = temp.CreateFile(Path.Combine("cache", "renodx-new.addon64"), "new-renodx");
        var originalPath = temp.CreateFile(Path.Combine("game", "renodx-old.addon64"), "not-a-pe-file");
        var game = CreateGame(gameDir);
        var renoDX = new RenoDX
        {
            OriginalName = "renodx-old.addon64",
            SelectedName = "renodx-new.addon64",
            Version = "20240202"
        };
        var sut = CreateSut();

        var result = await sut.InstallModAsync(game, renoDX, sourcePath);

        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(originalPath));
        Assert.Equal("new-renodx", File.ReadAllText(Path.Combine(gameDir, "renodx-new.addon64")));
        Assert.Same(renoDX, game.RenoDX);
    }

    // Verifies that a missing RenoDX source file returns a failure without changing game state.
    [Fact]
    public async Task InstallModAsync_WhenRenoDXSourceIsMissing_ReturnsFailure()
    {
        using var temp = new TempDirectory();
        var gameDir = temp.CreateDirectory("game");
        var game = CreateGame(gameDir);
        var renoDX = new RenoDX
        {
            OriginalName = "renodx-game.addon64",
            SelectedName = "renodx-game.addon64"
        };
        var sut = CreateSut();

        var result = await sut.InstallModAsync(game, renoDX, temp.GetPath("missing", "renodx-game.addon64"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Install failed. Disk may be full or the game folder was moved.", result.Message);
        Assert.Null(game.RenoDX);
        Assert.False(File.Exists(Path.Combine(gameDir, "renodx-game.addon64")));
    }

    // Verifies that ReShade uninstall deletes the selected file and clears the game state.
    [Fact]
    public async Task UninstallReShadeAsync_WhenFileExists_RemovesFileAndClearsGameState()
    {
        using var temp = new TempDirectory();
        var gameDir = temp.CreateDirectory("game");
        var installedPath = temp.CreateFile(Path.Combine("game", "dxgi.dll"), "reshade-content");
        var game = CreateGame(gameDir);
        game.ReShade = new ReShade { SelectedFilename = "dxgi.dll" };
        var sut = CreateSut();

        var result = await sut.UninstallReShadeAsync(game);

        Assert.True(result.IsSuccess);
        Assert.False(File.Exists(installedPath));
        Assert.Null(game.ReShade);
    }

    // Verifies that missing ReShade uninstall targets return a deep-scan failure.
    [Fact]
    public async Task UninstallReShadeAsync_WhenFileIsMissing_ReturnsFailureAndPromptsForDeepScan()
    {
        using var temp = new TempDirectory();
        var game = CreateGame(temp.CreateDirectory("game"));
        game.ReShade = new ReShade { SelectedFilename = "dxgi.dll" };
        var sut = CreateSut();

        var result = await sut.UninstallReShadeAsync(game);

        Assert.False(result.IsSuccess);
        Assert.True(result.ShouldPromptForDeepScan);
        Assert.Null(game.ReShade);
    }

    // Verifies that missing RenoDX uninstall targets return a deep-scan failure.
    [Fact]
    public async Task UninstallRenoDXAsync_WhenFileIsMissing_ReturnsFailureAndPromptsForDeepScan()
    {
        using var temp = new TempDirectory();
        var game = CreateGame(temp.CreateDirectory("game"));
        game.RenoDX = new RenoDX { SelectedName = "renodx-game.addon64" };
        var sut = CreateSut();

        var result = await sut.UninstallRenoDXAsync(game);

        Assert.False(result.IsSuccess);
        Assert.True(result.ShouldPromptForDeepScan);
        Assert.Null(game.RenoDX);
    }

    // Verifies that unverified RenoDX files are not deleted during uninstall.
    [Fact]
    public async Task UninstallRenoDXAsync_WhenFileIsNotVerifiedRenoDX_DoesNotDeleteFileButClearsGameState()
    {
        using var temp = new TempDirectory();
        var gameDir = temp.CreateDirectory("game");
        var installedPath = temp.CreateFile(Path.Combine("game", "renodx-game.addon64"), "not-a-pe-file");
        var game = CreateGame(gameDir);
        game.RenoDX = new RenoDX { SelectedName = "renodx-game.addon64" };
        var sut = CreateSut();

        var result = await sut.UninstallRenoDXAsync(game);

        Assert.False(result.IsSuccess);
        Assert.True(result.ShouldPromptForDeepScan);
        Assert.True(File.Exists(installedPath));
        Assert.Null(game.RenoDX);
    }

    // Verifies that removing all ReShade files in an empty folder still clears game state.
    [Fact]
    public async Task RemoveAllReShadeFiles_WhenDirectoryIsEmpty_ClearsGameState()
    {
        using var temp = new TempDirectory();
        var game = CreateGame(temp.CreateDirectory("game"));
        game.ReShade = new ReShade { SelectedFilename = "dxgi.dll" };
        var sut = CreateSut();

        var result = await sut.RemoveAllReShadeFiles(game);

        Assert.Same(game, result);
        Assert.Null(game.ReShade);
    }

    // Verifies that non-ReShade placeholder files are ignored during bulk ReShade removal.
    [Fact]
    public async Task RemoveAllReShadeFiles_WhenFilesAreNotVerifiedReShade_LeavesFilesAndClearsGameState()
    {
        using var temp = new TempDirectory();
        var gameDir = temp.CreateDirectory("game");
        var dllPath = temp.CreateFile(Path.Combine("game", "dxgi.dll"), "not-a-pe-file");
        var asiPath = temp.CreateFile(Path.Combine("game", "plugin.asi"), "not-a-pe-file");
        var game = CreateGame(gameDir);
        game.ReShade = new ReShade { SelectedFilename = "dxgi.dll" };
        var sut = CreateSut();

        var result = await sut.RemoveAllReShadeFiles(game);

        Assert.Same(game, result);
        Assert.True(File.Exists(dllPath));
        Assert.True(File.Exists(asiPath));
        Assert.Null(game.ReShade);
    }

    // Verifies that removing all RenoDX files in an empty folder still clears game state.
    [Fact]
    public async Task RemoveAllRenoDXFiles_WhenDirectoryIsEmpty_ClearsGameState()
    {
        using var temp = new TempDirectory();
        var game = CreateGame(temp.CreateDirectory("game"));
        game.RenoDX = new RenoDX { SelectedName = "renodx-game.addon64" };
        var sut = CreateSut();

        var result = await sut.RemoveAllRenoDXFiles(game);

        Assert.Same(game, result);
        Assert.Null(game.RenoDX);
    }

    // Verifies that non-RenoDX placeholder files are ignored during bulk RenoDX removal.
    [Fact]
    public async Task RemoveAllRenoDXFiles_WhenFilesAreNotVerifiedRenoDX_LeavesFilesAndClearsGameState()
    {
        using var temp = new TempDirectory();
        var gameDir = temp.CreateDirectory("game");
        var addon64Path = temp.CreateFile(Path.Combine("game", "renodx-game.addon64"), "not-a-pe-file");
        var addon32Path = temp.CreateFile(Path.Combine("game", "renodx-game.addon32"), "not-a-pe-file");
        var game = CreateGame(gameDir);
        game.RenoDX = new RenoDX { SelectedName = "renodx-game.addon64" };
        var sut = CreateSut();

        var result = await sut.RemoveAllRenoDXFiles(game);

        Assert.Same(game, result);
        Assert.True(File.Exists(addon64Path));
        Assert.True(File.Exists(addon32Path));
        Assert.Null(game.RenoDX);
    }

    private static Game CreateGame(string executablePath) =>
        new()
        {
            Name = "Test Game",
            ExecutablePath = executablePath
        };

    private static ModInstallService CreateSut() => new(new NoOpLogService());
}
