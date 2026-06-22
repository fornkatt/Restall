using Moq;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Application.UseCases;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Application;

public sealed class InstallRenoDXUseCaseTests
{
    // Verifies that x64 wiki mods use the x64 addon URL and filename.
    [Fact]
    public async Task ExecuteAsync_WhenSpecificWikiModSupportsX64_InstallsX64Addon()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var mod = CreateSpecificMod(snapshot64: "https://example.test/renodx-game.addon64");
        var request = CreateRequest(RenoDX.Architecture.x64, RenoDX.Branch.Wiki, modInfo: mod);

        var captured = await ExecuteSuccessfulInstallAsync(context, request);

        Assert.Equal("renodx-game.addon64", captured.OriginalName);
        Assert.Equal("renodx-game.addon64", captured.SelectedName);
        context.ModDownload.Verify(x => x.DownloadRenoDXAsync(
            RenoDX.Branch.Wiki,
            "renodx-game.addon64",
            null,
            "https://example.test/renodx-game.addon64",
            It.IsAny<IProgress<DownloadProgressReportDto>?>()), Times.Once);
    }

    // Verifies that x32 wiki installs fall back to x64 when no x32 addon exists.
    [Fact]
    public async Task ExecuteAsync_WhenSpecificWikiModLacksX32_FallsBackToX64Addon()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var mod = CreateSpecificMod(snapshot64: "https://example.test/renodx-game.addon64");
        var request = CreateRequest(RenoDX.Architecture.x32, RenoDX.Branch.Wiki, modInfo: mod);

        var captured = await ExecuteSuccessfulInstallAsync(context, request);

        Assert.Equal("renodx-game.addon64", captured.OriginalName);
        context.ModDownload.Verify(x => x.DownloadRenoDXAsync(
            RenoDX.Branch.Wiki,
            "renodx-game.addon64",
            null,
            null,
            It.IsAny<IProgress<DownloadProgressReportDto>?>()), Times.Once);
    }

    // Verifies that generic Unreal mods resolve to Unreal addon names for the requested architecture.
    [Theory]
    [InlineData(RenoDX.Architecture.x64, "renodx-unrealengine.addon64")]
    [InlineData(RenoDX.Architecture.x32, "renodx-unrealengine.addon32")]
    public async Task ExecuteAsync_WhenGenericUnrealModIsUsed_InstallsUnrealAddon(
        RenoDX.Architecture architecture,
        string expectedAddon)
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var generic = new RenoDXGenericModInfoDto("Generic Unreal", ":white_check_mark:", SupportedEngine.Unreal);
        var request = CreateRequest(architecture, RenoDX.Branch.Snapshot, genericModInfo: generic);

        var captured = await ExecuteSuccessfulInstallAsync(context, request);

        Assert.Equal(expectedAddon, captured.OriginalName);
        Assert.Equal(RenoDX.Branch.Snapshot, captured.BranchName);
        context.ModDownload.Verify(x => x.DownloadRenoDXAsync(
            RenoDX.Branch.Snapshot,
            expectedAddon,
            null,
            null,
            It.IsAny<IProgress<DownloadProgressReportDto>?>()), Times.Once);
    }

    // Verifies that generic Unity mods use the Unity download path and force Snapshot branch.
    [Fact]
    public async Task ExecuteAsync_WhenGenericUnityModIsUsed_UsesUnityDownloadAndSnapshotBranch()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var generic = new RenoDXGenericModInfoDto("Generic Unity", ":white_check_mark:", SupportedEngine.Unity);
        var request = CreateRequest(RenoDX.Architecture.x64, RenoDX.Branch.Nightly, genericModInfo: generic);

        var captured = await ExecuteSuccessfulInstallAsync(context, request);

        Assert.Equal("renodx-unityengine.addon64", captured.OriginalName);
        Assert.Equal(RenoDX.Branch.Snapshot, captured.BranchName);
        context.ModDownload.Verify(x => x.DownloadUnityRenoDXAsync(
            "renodx-unityengine.addon64",
            It.IsAny<IProgress<DownloadProgressReportDto>?>()), Times.Once);
        context.ModDownload.Verify(x => x.DownloadRenoDXAsync(
            It.IsAny<RenoDX.Branch>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<IProgress<DownloadProgressReportDto>?>()), Times.Never);
    }

    // Verifies that Unity and Unreal engine names are used when no wiki or generic mod is provided.
    [Theory]
    [InlineData(Game.Engine.Unity, RenoDX.Architecture.x64, "renodx-unityengine.addon64")]
    [InlineData(Game.Engine.Unreal, RenoDX.Architecture.x32, "renodx-unrealengine.addon32")]
    public async Task ExecuteAsync_WhenOnlyGameEngineIsKnown_UsesEngineBasedAddon(
        Game.Engine engine,
        RenoDX.Architecture architecture,
        string expectedAddon)
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var request = CreateRequest(architecture, RenoDX.Branch.Snapshot);
        request.Game.EngineName = engine;

        var captured = await ExecuteSuccessfulInstallAsync(context, request);

        Assert.Equal(expectedAddon, captured.OriginalName);
    }

    // Verifies that unknown games without mod metadata fail before download or install.
    [Fact]
    public async Task ExecuteAsync_WhenAddonFilenameCannotBeResolved_ReturnsFailureWithoutDownloadOrInstall()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var request = CreateRequest(RenoDX.Architecture.x64, RenoDX.Branch.Snapshot);

        var result = await context.Sut.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("Could not determine addon filename", result.Message);
        context.ModDownload.Verify(x => x.DownloadRenoDXAsync(
            It.IsAny<RenoDX.Branch>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<IProgress<DownloadProgressReportDto>?>()), Times.Never);
        context.ModDownload.Verify(x => x.DownloadUnityRenoDXAsync(
            It.IsAny<string>(),
            It.IsAny<IProgress<DownloadProgressReportDto>?>()), Times.Never);
        context.ModInstall.Verify(x => x.InstallModAsync(It.IsAny<Game>(), It.IsAny<RenoDX>(), It.IsAny<string>()), Times.Never);
    }

    // Verifies that reinstalling RenoDX keeps the previously selected filename.
    [Fact]
    public async Task ExecuteAsync_WhenGameAlreadyHasRenoDX_KeepsExistingSelectedName()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var mod = CreateSpecificMod(snapshot64: "https://example.test/renodx-game.addon64");
        var request = CreateRequest(RenoDX.Architecture.x64, RenoDX.Branch.Wiki, modInfo: mod);
        request.Game.RenoDX = new RenoDX
        {
            OriginalName = "renodx-game.addon64",
            SelectedName = "custom-renodx.addon64"
        };

        var captured = await ExecuteSuccessfulInstallAsync(context, request);

        Assert.Equal("renodx-game.addon64", captured.OriginalName);
        Assert.Equal("custom-renodx.addon64", captured.SelectedName);
    }

    // Verifies that outdated RenoDX cache files are deleted before download.
    [Fact]
    public async Task ExecuteAsync_WhenCachedVersionDiffersFromTarget_DeletesCachedFile()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var mod = CreateSpecificMod(snapshot64: "https://example.test/renodx-game.addon64");
        var request = CreateRequest(RenoDX.Architecture.x64, RenoDX.Branch.Snapshot, modInfo: mod, targetVersion: "20240202");
        var cachePath = temp.CreateFile(Path.Combine("renodx-cache", "Snapshot", "renodx-game.addon64"), "cached");
        context.ModDetection.SetupSequence(x => x.GetRenoDXFileVersion(cachePath))
            .Returns("20240101")
            .Returns("20240202");

        await ExecuteSuccessfulInstallAsync(context, request, setupDefaultVersionDetection: false);

        Assert.False(File.Exists(cachePath));
    }

    // Verifies that RenoDX cache files are kept when target version is absent, invalid, or already current.
    [Theory]
    [InlineData(null, "20240101")]
    [InlineData("not-a-date", "20240101")]
    [InlineData("20240101", "20240101")]
    public async Task ExecuteAsync_WhenCacheInvalidationIsNotRequired_KeepsCachedFile(
        string? targetVersion,
        string cachedVersion)
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var mod = CreateSpecificMod(snapshot64: "https://example.test/renodx-game.addon64");
        var request = CreateRequest(RenoDX.Architecture.x64, RenoDX.Branch.Snapshot, modInfo: mod, targetVersion: targetVersion);
        var cachePath = temp.CreateFile(Path.Combine("renodx-cache", "Snapshot", "renodx-game.addon64"), "cached");
        context.ModDetection.SetupSequence(x => x.GetRenoDXFileVersion(cachePath))
            .Returns(cachedVersion)
            .Returns(cachedVersion);

        await ExecuteSuccessfulInstallAsync(context, request, setupDefaultVersionDetection: false);

        Assert.True(File.Exists(cachePath));
    }

    // Verifies that failed RenoDX downloads stop version detection and installation.
    [Fact]
    public async Task ExecuteAsync_WhenDownloadFails_ReturnsFailureWithoutDetectingVersionOrInstalling()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var mod = CreateSpecificMod(snapshot64: "https://example.test/renodx-game.addon64");
        var request = CreateRequest(RenoDX.Architecture.x64, RenoDX.Branch.Snapshot, modInfo: mod);

        context.ModDownload.Setup(x => x.DownloadRenoDXAsync(
                RenoDX.Branch.Snapshot,
                "renodx-game.addon64",
                null,
                "https://example.test/renodx-game.addon64",
                It.IsAny<IProgress<DownloadProgressReportDto>?>()))
            .ReturnsAsync(false);

        var result = await context.Sut.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed to download file.", result.Message);
        context.ModDetection.Verify(x => x.GetRenoDXFileVersion(It.IsAny<string>()), Times.Never);
        context.ModInstall.Verify(x => x.InstallModAsync(It.IsAny<Game>(), It.IsAny<RenoDX>(), It.IsAny<string>()), Times.Never);
    }

    private static async Task<RenoDX> ExecuteSuccessfulInstallAsync(
        RenoDXContext context,
        InstallRenoDXRequest request,
        bool setupDefaultVersionDetection = true)
    {
        RenoDX? captured = null;

        context.ModDownload.Setup(x => x.DownloadRenoDXAsync(
                It.IsAny<RenoDX.Branch>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<IProgress<DownloadProgressReportDto>?>()))
            .ReturnsAsync(true);
        context.ModDownload.Setup(x => x.DownloadUnityRenoDXAsync(
                It.IsAny<string>(),
                It.IsAny<IProgress<DownloadProgressReportDto>?>()))
            .ReturnsAsync(true);
        if (setupDefaultVersionDetection)
            context.ModDetection.Setup(x => x.GetRenoDXFileVersion(It.IsAny<string>())).Returns("20240202");
        context.ModInstall.Setup(x => x.InstallModAsync(request.Game, It.IsAny<RenoDX>(), It.IsAny<string>()))
            .Callback<Game, RenoDX, string>((_, renoDX, _) => captured = renoDX)
            .ReturnsAsync((Game game, RenoDX renoDX, string _) =>
            {
                game.RenoDX = renoDX;
                return new ModOperationResultDto(true, game);
            });

        var result = await context.Sut.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        return captured;
    }

    private static InstallRenoDXRequest CreateRequest(
        RenoDX.Architecture architecture,
        RenoDX.Branch branch,
        RenoDXModInfoDto? modInfo = null,
        RenoDXGenericModInfoDto? genericModInfo = null,
        string? targetVersion = null) =>
        new(
            new Game { Name = "Test Game", EngineName = Game.Engine.Unknown },
            architecture,
            branch,
            modInfo,
            genericModInfo,
            targetVersion);

    private static RenoDXModInfoDto CreateSpecificMod(string? snapshot64 = null, string? snapshot32 = null) =>
        new(
            "Test Game",
            DiscordUrl: null,
            SnapshotUrl64: snapshot64,
            SnapshotUrl32: snapshot32,
            NexusUrl: null,
            Maintainer: "Maintainer",
            Notes: null,
            Status: ":white_check_mark:");

    private static RenoDXContext CreateContext(TempDirectory temp)
    {
        var modDownload = new Mock<IModDownloadService>(MockBehavior.Strict);
        var modInstall = new Mock<IModInstallService>(MockBehavior.Strict);
        var modDetection = new Mock<IModDetectionService>(MockBehavior.Strict);
        var pathService = new Mock<IPathService>(MockBehavior.Strict);

        pathService.Setup(x => x.GetRenoDXCachePath(It.IsAny<RenoDX>()))
            .Returns((RenoDX renoDX) => temp.GetPath("renodx-cache", renoDX.BranchName.ToString(), renoDX.OriginalName!));

        var sut = new InstallRenoDXUseCase(
            modDownload.Object,
            modInstall.Object,
            modDetection.Object,
            pathService.Object,
            new NoOpLogService());

        return new RenoDXContext(sut, modDownload, modInstall, modDetection, pathService);
    }

    private sealed record RenoDXContext(
        InstallRenoDXUseCase Sut,
        Mock<IModDownloadService> ModDownload,
        Mock<IModInstallService> ModInstall,
        Mock<IModDetectionService> ModDetection,
        Mock<IPathService> PathService);
}
