using Moq;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Application.UseCases;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Application;

public sealed class InstallReShadeUseCaseTests
{
    // Verifies that cached extracted ReShade files are installed without download or extraction.
    [Fact]
    public async Task ExecuteAsync_WhenExtractedFileExists_InstallsWithoutDownloadOrExtract()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var request = CreateRequest();
        var extractedPath = temp.CreateFile(Path.Combine("cache", "ReShade64.dll"), "reshade");
        context.PathService.Setup(x => x.GetReShadeExtractedFilePath(It.IsAny<ReShade>())).Returns(extractedPath);
        context.ModInstall.Setup(x => x.InstallModAsync(request.Game, It.IsAny<ReShade>(), extractedPath))
            .ReturnsAsync((Game game, ReShade reShade, string _) =>
            {
                game.ReShade = reShade;
                return new ModOperationResultDto(true, game);
            });

        var result = await context.Sut.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(request.SelectedFilename, result.UpdatedGame.ReShade?.SelectedFilename);
        context.ModDownload.Verify(x => x.DownloadReShadeAsync(
            It.IsAny<ReShade.Branch>(),
            It.IsAny<string>(),
            It.IsAny<IProgress<DownloadProgressReportDto>?>()), Times.Never);
        context.FileExtraction.Verify(x => x.ExtractFiles(
            It.IsAny<string>(),
            It.IsAny<string[]>(),
            It.IsAny<string>()), Times.Never);
    }

    // Verifies that cached installers skip download but still extract and install when extracted files are missing.
    [Fact]
    public async Task ExecuteAsync_WhenInstallerExistsButExtractedFileIsMissing_ExtractsAndInstalls()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var request = CreateRequest();
        var installerPath = temp.CreateFile(Path.Combine("downloads", "ReShade_Setup_6.5.0_Addon.exe"), "installer");
        var extractedPath = temp.GetPath("cache", "Stable", "6.5.0", "ReShade64.dll");
        var extractionDir = Path.GetDirectoryName(extractedPath)!;

        context.PathService.Setup(x => x.GetReShadeExtractedFilePath(It.IsAny<ReShade>())).Returns(extractedPath);
        context.PathService.Setup(x => x.GetReShadeInstallerFilePath(ReShade.Branch.Stable, "6.5.0")).Returns(installerPath);
        context.PathService.Setup(x => x.GetReShadeCachePath(It.IsAny<ReShade>())).Returns(extractionDir);
        context.FileExtraction.Setup(x => x.ExtractFiles(
                installerPath,
                It.Is<string[]>(files => files.SequenceEqual(new[] { "ReShade64.dll" })),
                extractionDir))
            .Returns(true);
        context.ModInstall.Setup(x => x.InstallModAsync(request.Game, It.IsAny<ReShade>(), extractedPath))
            .ReturnsAsync(new ModOperationResultDto(true, request.Game));

        var result = await context.Sut.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        context.ModDownload.Verify(x => x.DownloadReShadeAsync(
            It.IsAny<ReShade.Branch>(),
            It.IsAny<string>(),
            It.IsAny<IProgress<DownloadProgressReportDto>?>()), Times.Never);
        context.FileExtraction.VerifyAll();
        context.ModInstall.Verify(x => x.InstallModAsync(request.Game, It.IsAny<ReShade>(), extractedPath), Times.Once);
    }

    // Verifies that failed ReShade downloads stop before extraction and installation.
    [Fact]
    public async Task ExecuteAsync_WhenDownloadFails_ReturnsFailureWithoutExtractOrInstall()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var request = CreateRequest();
        context.PathService.Setup(x => x.GetReShadeExtractedFilePath(It.IsAny<ReShade>()))
            .Returns(temp.GetPath("cache", "ReShade64.dll"));
        context.PathService.Setup(x => x.GetReShadeInstallerFilePath(ReShade.Branch.Stable, "6.5.0"))
            .Returns(temp.GetPath("downloads", "ReShade_Setup_6.5.0_Addon.exe"));
        context.ModDownload.Setup(x => x.DownloadReShadeAsync(
                ReShade.Branch.Stable,
                "6.5.0",
                It.IsAny<IProgress<DownloadProgressReportDto>?>()))
            .ReturnsAsync(false);

        var result = await context.Sut.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed to download ReShade installer.", result.Message);
        context.FileExtraction.Verify(x => x.ExtractFiles(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>()), Times.Never);
        context.ModInstall.Verify(x => x.InstallModAsync(It.IsAny<Game>(), It.IsAny<ReShade>(), It.IsAny<string>()), Times.Never);
    }

    // Verifies that failed ReShade extraction stops before installation.
    [Fact]
    public async Task ExecuteAsync_WhenExtractionFails_ReturnsFailureWithoutInstall()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var request = CreateRequest();
        var installerPath = temp.GetPath("downloads", "ReShade_Setup_6.5.0_Addon.exe");
        var extractionDir = temp.GetPath("cache", "Stable", "6.5.0");

        context.PathService.Setup(x => x.GetReShadeExtractedFilePath(It.IsAny<ReShade>()))
            .Returns(Path.Combine(extractionDir, "ReShade64.dll"));
        context.PathService.Setup(x => x.GetReShadeInstallerFilePath(ReShade.Branch.Stable, "6.5.0")).Returns(installerPath);
        context.PathService.Setup(x => x.GetReShadeCachePath(It.IsAny<ReShade>())).Returns(extractionDir);
        context.ModDownload.Setup(x => x.DownloadReShadeAsync(
                ReShade.Branch.Stable,
                "6.5.0",
                It.IsAny<IProgress<DownloadProgressReportDto>?>()))
            .ReturnsAsync(true);
        context.FileExtraction.Setup(x => x.ExtractFiles(installerPath, It.IsAny<string[]>(), extractionDir)).Returns(false);

        var result = await context.Sut.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed to extract files from installer.", result.Message);
        context.ModInstall.Verify(x => x.InstallModAsync(It.IsAny<Game>(), It.IsAny<ReShade>(), It.IsAny<string>()), Times.Never);
    }

    // Verifies that install failures are returned after a valid ReShade package is prepared.
    [Fact]
    public async Task ExecuteAsync_WhenInstallFails_ReturnsInstallResult()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var request = CreateRequest();
        var extractedPath = temp.CreateFile(Path.Combine("cache", "ReShade64.dll"), "reshade");
        var expected = new ModOperationResultDto(false, request.Game, "install failed");

        context.PathService.Setup(x => x.GetReShadeExtractedFilePath(It.IsAny<ReShade>())).Returns(extractedPath);
        context.ModInstall.Setup(x => x.InstallModAsync(request.Game, It.IsAny<ReShade>(), extractedPath))
            .ReturnsAsync(expected);

        var result = await context.Sut.ExecuteAsync(request);

        Assert.Same(expected, result);
    }

    // Verifies that request data is mapped onto the ReShade model passed to the installer.
    [Fact]
    public async Task ExecuteAsync_MapsRequestDataToInstalledReShade()
    {
        using var temp = new TempDirectory();
        var context = CreateContext(temp);
        var request = new InstallReShadeRequest(
            new Game { Name = "Test Game" },
            ReShade.Branch.Nightly,
            ReShade.Architecture.x32,
            "6.4.1",
            "d3d9.asi");
        var extractedPath = temp.CreateFile(Path.Combine("cache", "ReShade32.dll"), "reshade");
        ReShade? captured = null;

        context.PathService.Setup(x => x.GetReShadeExtractedFilePath(It.IsAny<ReShade>())).Returns(extractedPath);
        context.ModInstall.Setup(x => x.InstallModAsync(request.Game, It.IsAny<ReShade>(), extractedPath))
            .Callback<Game, ReShade, string>((_, reShade, _) => captured = reShade)
            .ReturnsAsync(new ModOperationResultDto(true, request.Game));

        await context.Sut.ExecuteAsync(request);

        Assert.NotNull(captured);
        Assert.Equal(ReShade.Branch.Nightly, captured.BranchName);
        Assert.Equal(ReShade.Architecture.x32, captured.Arch);
        Assert.Equal("6.4.1", captured.Version);
        Assert.Equal("d3d9.asi", captured.SelectedFilename);
    }

    private static InstallReShadeRequest CreateRequest() =>
        new(new Game { Name = "Test Game" }, ReShade.Branch.Stable, ReShade.Architecture.x64, "6.5.0", "dxgi.dll");

    private static ReShadeContext CreateContext(TempDirectory temp)
    {
        var pathService = new Mock<IPathService>(MockBehavior.Strict);
        var modDownload = new Mock<IModDownloadService>(MockBehavior.Strict);
        var fileExtraction = new Mock<IFileExtractionService>(MockBehavior.Strict);
        var modInstall = new Mock<IModInstallService>(MockBehavior.Strict);

        pathService.Setup(x => x.GetReShadeInstallerFilePath(It.IsAny<ReShade.Branch>(), It.IsAny<string>()))
            .Returns((ReShade.Branch branch, string version) =>
                temp.GetPath("downloads", branch.ToString(), $"ReShade_Setup_{version}_Addon.exe"));
        pathService.Setup(x => x.GetReShadeCachePath(It.IsAny<ReShade>()))
            .Returns((ReShade reShade) => temp.GetPath("cache", reShade.BranchName.ToString(), reShade.Version!));

        var sut = new InstallReShadeUseCase(
            pathService.Object,
            modDownload.Object,
            fileExtraction.Object,
            modInstall.Object,
            new NoOpLogService());

        return new ReShadeContext(sut, pathService, modDownload, fileExtraction, modInstall);
    }

    private sealed record ReShadeContext(
        InstallReShadeUseCase Sut,
        Mock<IPathService> PathService,
        Mock<IModDownloadService> ModDownload,
        Mock<IFileExtractionService> FileExtraction,
        Mock<IModInstallService> ModInstall);
}
