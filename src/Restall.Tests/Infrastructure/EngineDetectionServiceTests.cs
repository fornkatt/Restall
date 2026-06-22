using Restall.Domain.Entities;
using Restall.Infrastructure.Services;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Infrastructure;

public sealed class EngineDetectionServiceTests
{
    // Verifies that Unreal Binaries/Win64 folders with executables are detected as Unreal.
    [Fact]
    public void DetectExecutablePathAndEngine_WhenUnrealBinariesExist_ReturnsUnrealBinariesFolder()
    {
        using var temp = new TempDirectory();
        var binaries = temp.CreateDirectory(Path.Combine("Game", "Binaries", "Win64"));
        temp.CreateFile(Path.Combine("Game", "Binaries", "Win64", "Game.exe"));
        var sut = CreateSut();

        var (executablePath, engine) = sut.DetectExecutablePathAndEngine(temp.GetPath("Game"));

        Assert.Equal(binaries, executablePath);
        Assert.Equal(Game.Engine.Unreal, engine);
    }

    // Verifies that Unreal detection prefers candidate folders containing Shipping executables.
    [Fact]
    public void DetectExecutablePathAndEngine_WhenMultipleUnrealBinariesExist_PrefersShippingExecutable()
    {
        using var temp = new TempDirectory();
        temp.CreateFile(Path.Combine("Root", "First", "Binaries", "Win64", "Tool.exe"));
        var shipping = temp.CreateDirectory(Path.Combine("Root", "Second", "Binaries", "Win64"));
        temp.CreateFile(Path.Combine("Root", "Second", "Binaries", "Win64", "Game-Win64-Shipping.exe"));
        var sut = CreateSut();

        var (executablePath, engine) = sut.DetectExecutablePathAndEngine(temp.GetPath("Root"));

        Assert.Equal(shipping, executablePath);
        Assert.Equal(Game.Engine.Unreal, engine);
    }

    // Verifies that UnityPlayer.dll within the shallow search depth is detected as Unity.
    [Fact]
    public void DetectExecutablePathAndEngine_WhenUnityPlayerExists_ReturnsUnityFolder()
    {
        using var temp = new TempDirectory();
        var unityFolder = temp.CreateDirectory(Path.Combine("Game", "Managed"));
        temp.CreateFile(Path.Combine("Game", "Managed", "UnityPlayer.dll"));
        var sut = CreateSut();

        var (executablePath, engine) = sut.DetectExecutablePathAndEngine(temp.GetPath("Game"));

        Assert.Equal(unityFolder, executablePath);
        Assert.Equal(Game.Engine.Unity, engine);
    }

    // Verifies that preferred executable subfolders are chosen for unknown engines.
    [Theory]
    [InlineData("bin\\x64")]
    [InlineData("bin\\x86")]
    [InlineData("bin\\win64")]
    public void DetectExecutablePathAndEngine_WhenPreferredExeFolderExists_ReturnsPreferredFolder(string subFolder)
    {
        using var temp = new TempDirectory();
        var exeFolder = temp.CreateDirectory(Path.Combine("Game", subFolder));
        temp.CreateFile(Path.Combine("Game", subFolder, "Game.exe"));
        var sut = CreateSut();

        var (executablePath, engine) = sut.DetectExecutablePathAndEngine(temp.GetPath("Game"));

        Assert.Equal(exeFolder, executablePath);
        Assert.Equal(Game.Engine.Unknown, engine);
    }

    // Verifies that shallow BFS finds an executable when preferred folders are absent.
    [Fact]
    public void DetectExecutablePathAndEngine_WhenOnlyShallowExeExists_ReturnsExeFolderAsUnknownEngine()
    {
        using var temp = new TempDirectory();
        var exeFolder = temp.CreateDirectory(Path.Combine("Game", "Sub", "Content"));
        temp.CreateFile(Path.Combine("Game", "Sub", "Content", "Game.exe"));
        var sut = CreateSut();

        var (executablePath, engine) = sut.DetectExecutablePathAndEngine(temp.GetPath("Game"));

        Assert.Equal(exeFolder, executablePath);
        Assert.Equal(Game.Engine.Unknown, engine);
    }

    // Verifies that non-game folders are skipped during executable folder discovery.
    [Fact]
    public void DetectExecutablePathAndEngine_WhenExeExistsOnlyInNonGameFolder_ReturnsNullUnknown()
    {
        using var temp = new TempDirectory();
        temp.CreateFile(Path.Combine("Game", "_CommonRedist", "Helper.exe"));
        var sut = CreateSut();

        var (executablePath, engine) = sut.DetectExecutablePathAndEngine(temp.GetPath("Game"));

        Assert.Null(executablePath);
        Assert.Equal(Game.Engine.Unknown, engine);
    }

    // Verifies that folders without recognizable engine or executable files return null unknown.
    [Fact]
    public void DetectExecutablePathAndEngine_WhenNoEngineOrExecutableExists_ReturnsNullUnknown()
    {
        using var temp = new TempDirectory();
        temp.CreateDirectory("Game");
        var sut = CreateSut();

        var (executablePath, engine) = sut.DetectExecutablePathAndEngine(temp.GetPath("Game"));

        Assert.Null(executablePath);
        Assert.Equal(Game.Engine.Unknown, engine);
    }

    private static EngineDetectionService CreateSut() => new(new NoOpLogService());
}
