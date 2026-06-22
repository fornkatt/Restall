using Moq;
using Restall.Application.DTOs;
using Restall.Application.Facades;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.UseCases.Requests;
using Restall.Domain.Entities;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Application;

public sealed class ModManagementFacadeTests
{
    // Verifies that ReShade installation stops before the use case when the game folder is missing.
    [Fact]
    public async Task InstallOrUpdateReShadeAsync_WhenGameFolderIsMissing_ReturnsErrorWithoutCallingUseCase()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.GetPath("missing"));

        var result = await context.Sut.InstallOrUpdateReShadeAsync(CreateReShadeRequest(game));

        Assert.False(result.IsSuccess);
        Assert.Equal("Game folder not found. Please rescan your library.", result.Message);
        context.InstallReShade.Verify(
            x => x.ExecuteAsync(It.IsAny<InstallReShadeRequest>(), It.IsAny<IProgress<DownloadProgressReportDto>?>()),
            Times.Never);
    }

    // Verifies that a stale ReShade record blocks installation and requests a deep scan.
    [Fact]
    public async Task InstallOrUpdateReShadeAsync_WhenRecordedFileIsMissing_ReturnsStaleRecordError()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.CreateDirectory("game"));
        game.ReShade = new ReShade { SelectedFilename = "dxgi.dll", Version = "6.4.0" };

        var result = await context.Sut.InstallOrUpdateReShadeAsync(CreateReShadeRequest(game));

        Assert.False(result.IsSuccess);
        Assert.True(result.ShouldPromptForDeepScan);
        Assert.Contains("dxgi.dll", result.Message);
        context.InstallReShade.Verify(
            x => x.ExecuteAsync(It.IsAny<InstallReShadeRequest>(), It.IsAny<IProgress<DownloadProgressReportDto>?>()),
            Times.Never);
    }

    // Verifies that successful ReShade installation is enriched with update-check state.
    [Fact]
    public async Task InstallOrUpdateReShadeAsync_WhenInstallSucceeds_AddsUpdateCheckResult()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.CreateDirectory("game"));
        var installed = new ReShade { SelectedFilename = "dxgi.dll", Version = "6.5.0" };
        var updatedGame = CreateGame(game.ExecutablePath!);
        updatedGame.ReShade = installed;
        var updateResult = new UpdateCheckResultDto(false, "6.5.0", "6.5.0");

        context.InstallReShade
            .Setup(x => x.ExecuteAsync(
                It.IsAny<InstallReShadeRequest>(),
                It.IsAny<IProgress<DownloadProgressReportDto>?>()))
            .ReturnsAsync(new ModOperationResultDto(true, updatedGame));
        context.UpdateCheck.Setup(x => x.CheckReShadeUpdate(installed)).Returns(updateResult);

        var result = await context.Sut.InstallOrUpdateReShadeAsync(CreateReShadeRequest(game));

        Assert.True(result.IsSuccess);
        Assert.Same(updateResult, result.UpdateCheckResult);
        context.UpdateCheck.Verify(x => x.CheckReShadeUpdate(installed), Times.Once);
    }

    // Verifies that failed ReShade installation does not run update checking.
    [Fact]
    public async Task InstallOrUpdateReShadeAsync_WhenInstallFails_DoesNotRunUpdateCheck()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.CreateDirectory("game"));

        context.InstallReShade
            .Setup(x => x.ExecuteAsync(
                It.IsAny<InstallReShadeRequest>(),
                It.IsAny<IProgress<DownloadProgressReportDto>?>()))
            .ReturnsAsync(new ModOperationResultDto(false, game, "failed"));

        var result = await context.Sut.InstallOrUpdateReShadeAsync(CreateReShadeRequest(game));

        Assert.False(result.IsSuccess);
        Assert.Equal("failed", result.Message);
        context.UpdateCheck.Verify(x => x.CheckReShadeUpdate(It.IsAny<ReShade>()), Times.Never);
    }

    // Verifies that ReShade uninstall stops before the use case when the game folder is missing.
    [Fact]
    public async Task UninstallReShadeAsync_WhenGameFolderIsMissing_ReturnsErrorWithoutCallingUseCase()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.GetPath("missing"));
        game.ReShade = new ReShade { SelectedFilename = "dxgi.dll" };

        var result = await context.Sut.UninstallReShadeAsync(game);

        Assert.False(result.IsSuccess);
        Assert.Equal("Game folder not found. Please rescan your library.", result.Message);
        context.UninstallReShade.Verify(x => x.ExecuteAsync(It.IsAny<Game>()), Times.Never);
    }

    // Verifies that ReShade uninstall fails early when no ReShade record exists.
    [Fact]
    public async Task UninstallReShadeAsync_WhenNoReShadeIsRecorded_ReturnsErrorWithoutCallingUseCase()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.CreateDirectory("game"));

        var result = await context.Sut.UninstallReShadeAsync(game);

        Assert.False(result.IsSuccess);
        Assert.Contains("No ReShade installation detected", result.Message);
        context.UninstallReShade.Verify(x => x.ExecuteAsync(It.IsAny<Game>()), Times.Never);
    }

    // Verifies that ReShade uninstall delegates to the use case when a record exists.
    [Fact]
    public async Task UninstallReShadeAsync_WhenReShadeIsRecorded_CallsUseCase()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.CreateDirectory("game"));
        game.ReShade = new ReShade { SelectedFilename = "dxgi.dll" };
        var expected = new ModOperationResultDto(true, game, "ok");

        context.UninstallReShade.Setup(x => x.ExecuteAsync(game)).ReturnsAsync(expected);

        var result = await context.Sut.UninstallReShadeAsync(game);

        Assert.Same(expected, result);
        context.UninstallReShade.Verify(x => x.ExecuteAsync(game), Times.Once);
    }

    // Verifies that RenoDX installation stops before the use case when the game folder is missing.
    [Fact]
    public async Task InstallOrUpdateRenoDXAsync_WhenGameFolderIsMissing_ReturnsErrorWithoutCallingUseCase()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.GetPath("missing"));

        var result = await context.Sut.InstallOrUpdateRenoDXAsync(CreateRenoDXRequest(game));

        Assert.False(result.IsSuccess);
        Assert.Equal("Game folder not found. Please rescan your library.", result.Message);
        context.InstallRenoDX.Verify(
            x => x.ExecuteAsync(It.IsAny<InstallRenoDXRequest>(), It.IsAny<IProgress<DownloadProgressReportDto>?>()),
            Times.Never);
    }

    // Verifies that a stale RenoDX record blocks installation and requests a deep scan.
    [Fact]
    public async Task InstallOrUpdateRenoDXAsync_WhenRecordedFileIsMissing_ReturnsStaleRecordError()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.CreateDirectory("game"));
        game.RenoDX = new RenoDX { SelectedName = "renodx-game.addon64", Version = "20240101" };

        var result = await context.Sut.InstallOrUpdateRenoDXAsync(CreateRenoDXRequest(game));

        Assert.False(result.IsSuccess);
        Assert.True(result.ShouldPromptForDeepScan);
        Assert.Contains("renodx-game.addon64", result.Message);
        context.InstallRenoDX.Verify(
            x => x.ExecuteAsync(It.IsAny<InstallRenoDXRequest>(), It.IsAny<IProgress<DownloadProgressReportDto>?>()),
            Times.Never);
    }

    // Verifies that successful RenoDX installation is enriched with update-check state.
    [Fact]
    public async Task InstallOrUpdateRenoDXAsync_WhenInstallSucceeds_AddsUpdateCheckResult()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.CreateDirectory("game"));
        var installed = new RenoDX
        {
            SelectedName = "renodx-game.addon64",
            OriginalName = "renodx-game.addon64",
            BranchName = RenoDX.Branch.Snapshot,
            Version = "20240101"
        };
        var updatedGame = CreateGame(game.ExecutablePath!);
        updatedGame.RenoDX = installed;
        var updateResult = new UpdateCheckResultDto(true, "20240101", "20240202");

        context.InstallRenoDX
            .Setup(x => x.ExecuteAsync(
                It.IsAny<InstallRenoDXRequest>(),
                It.IsAny<IProgress<DownloadProgressReportDto>?>()))
            .ReturnsAsync(new ModOperationResultDto(true, updatedGame));
        context.UpdateCheck.Setup(x => x.CheckRenoDXUpdate(installed)).Returns(updateResult);

        var result = await context.Sut.InstallOrUpdateRenoDXAsync(CreateRenoDXRequest(game));

        Assert.True(result.IsSuccess);
        Assert.Same(updateResult, result.UpdateCheckResult);
        context.UpdateCheck.Verify(x => x.CheckRenoDXUpdate(installed), Times.Once);
    }

    // Verifies that failed RenoDX installation does not run update checking.
    [Fact]
    public async Task InstallOrUpdateRenoDXAsync_WhenInstallFails_DoesNotRunUpdateCheck()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.CreateDirectory("game"));

        context.InstallRenoDX
            .Setup(x => x.ExecuteAsync(
                It.IsAny<InstallRenoDXRequest>(),
                It.IsAny<IProgress<DownloadProgressReportDto>?>()))
            .ReturnsAsync(new ModOperationResultDto(false, game, "failed"));

        var result = await context.Sut.InstallOrUpdateRenoDXAsync(CreateRenoDXRequest(game));

        Assert.False(result.IsSuccess);
        Assert.Equal("failed", result.Message);
        context.UpdateCheck.Verify(x => x.CheckRenoDXUpdate(It.IsAny<RenoDX>()), Times.Never);
    }

    // Verifies that RenoDX uninstall stops before the use case when the game folder is missing.
    [Fact]
    public async Task UninstallRenoDXAsync_WhenGameFolderIsMissing_ReturnsErrorWithoutCallingUseCase()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.GetPath("missing"));
        game.RenoDX = new RenoDX { SelectedName = "renodx-game.addon64" };

        var result = await context.Sut.UninstallRenoDXAsync(game);

        Assert.False(result.IsSuccess);
        Assert.Equal("Game folder not found. Please rescan your library.", result.Message);
        context.UninstallRenoDX.Verify(x => x.ExecuteAsync(It.IsAny<Game>()), Times.Never);
    }

    // Verifies that RenoDX uninstall fails early when no RenoDX record exists.
    [Fact]
    public async Task UninstallRenoDXAsync_WhenNoRenoDXIsRecorded_ReturnsErrorWithoutCallingUseCase()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.CreateDirectory("game"));

        var result = await context.Sut.UninstallRenoDXAsync(game);

        Assert.False(result.IsSuccess);
        Assert.Contains("No RenoDX installation detected", result.Message);
        context.UninstallRenoDX.Verify(x => x.ExecuteAsync(It.IsAny<Game>()), Times.Never);
    }

    // Verifies that RenoDX uninstall delegates to the use case when a record exists.
    [Fact]
    public async Task UninstallRenoDXAsync_WhenRenoDXIsRecorded_CallsUseCase()
    {
        using var temp = new TempDirectory();
        var context = CreateContext();
        var game = CreateGame(temp.CreateDirectory("game"));
        game.RenoDX = new RenoDX { SelectedName = "renodx-game.addon64" };
        var expected = new ModOperationResultDto(true, game, "ok");

        context.UninstallRenoDX.Setup(x => x.ExecuteAsync(game)).ReturnsAsync(expected);

        var result = await context.Sut.UninstallRenoDXAsync(game);

        Assert.Same(expected, result);
        context.UninstallRenoDX.Verify(x => x.ExecuteAsync(game), Times.Once);
    }

    private static Game CreateGame(string executablePath) =>
        new()
        {
            Name = "Test Game",
            ExecutablePath = executablePath
        };

    private static InstallReShadeRequest CreateReShadeRequest(Game game) =>
        new(game, ReShade.Branch.Stable, ReShade.Architecture.x64, "6.5.0", "dxgi.dll");

    private static InstallRenoDXRequest CreateRenoDXRequest(Game game) =>
        new(game, RenoDX.Architecture.x64, RenoDX.Branch.Snapshot);

    private static FacadeContext CreateContext()
    {
        var installReShade = new Mock<IInstallReShadeUseCase>(MockBehavior.Strict);
        var uninstallReShade = new Mock<IUninstallReShadeUseCase>(MockBehavior.Strict);
        var installRenoDX = new Mock<IInstallRenoDXUseCase>(MockBehavior.Strict);
        var uninstallRenoDX = new Mock<IUninstallRenoDXUseCase>(MockBehavior.Strict);
        var updateCheck = new Mock<IUpdateCheckService>(MockBehavior.Strict);

        var sut = new ModManagementFacade(
            installReShade.Object,
            uninstallReShade.Object,
            installRenoDX.Object,
            uninstallRenoDX.Object,
            updateCheck.Object);

        return new FacadeContext(
            sut,
            installReShade,
            uninstallReShade,
            installRenoDX,
            uninstallRenoDX,
            updateCheck);
    }

    private sealed record FacadeContext(
        ModManagementFacade Sut,
        Mock<IInstallReShadeUseCase> InstallReShade,
        Mock<IUninstallReShadeUseCase> UninstallReShade,
        Mock<IInstallRenoDXUseCase> InstallRenoDX,
        Mock<IUninstallRenoDXUseCase> UninstallRenoDX,
        Mock<IUpdateCheckService> UpdateCheck);
}
