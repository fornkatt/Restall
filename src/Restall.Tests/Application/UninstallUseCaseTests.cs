using Moq;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Application.UseCases;
using Restall.Domain.Entities;

namespace Restall.Tests.Application;

public sealed class UninstallUseCaseTests
{
    // Verifies that ReShade uninstall delegates to the mod install service with the same game instance.
    [Fact]
    public async Task UninstallReShadeUseCase_ExecuteAsync_CallsModInstallServiceWithSameGame()
    {
        var game = new Game { Name = "Game" };
        var expected = new ModOperationResultDto(true, game);
        var modInstall = new Mock<IModInstallService>(MockBehavior.Strict);
        modInstall.Setup(x => x.UninstallReShadeAsync(game)).ReturnsAsync(expected);
        var sut = new UninstallReShadeUseCase(modInstall.Object);

        var result = await sut.ExecuteAsync(game);

        Assert.Same(expected, result);
        modInstall.Verify(x => x.UninstallReShadeAsync(game), Times.Once);
    }

    // Verifies that ReShade uninstall failures are returned unchanged.
    [Fact]
    public async Task UninstallReShadeUseCase_ExecuteAsync_WhenServiceFails_ReturnsFailureResult()
    {
        var game = new Game { Name = "Game" };
        var expected = new ModOperationResultDto(false, game, "Failed", ShouldPromptForDeepScan: true);
        var modInstall = new Mock<IModInstallService>(MockBehavior.Strict);
        modInstall.Setup(x => x.UninstallReShadeAsync(game)).ReturnsAsync(expected);
        var sut = new UninstallReShadeUseCase(modInstall.Object);

        var result = await sut.ExecuteAsync(game);

        Assert.Same(expected, result);
        Assert.False(result.IsSuccess);
        Assert.True(result.ShouldPromptForDeepScan);
    }

    // Verifies that RenoDX uninstall delegates to the mod install service with the same game instance.
    [Fact]
    public async Task UninstallRenoDXUseCase_ExecuteAsync_CallsModInstallServiceWithSameGame()
    {
        var game = new Game { Name = "Game" };
        var expected = new ModOperationResultDto(true, game);
        var modInstall = new Mock<IModInstallService>(MockBehavior.Strict);
        modInstall.Setup(x => x.UninstallRenoDXAsync(game)).ReturnsAsync(expected);
        var sut = new UninstallRenoDXUseCase(modInstall.Object);

        var result = await sut.ExecuteAsync(game);

        Assert.Same(expected, result);
        modInstall.Verify(x => x.UninstallRenoDXAsync(game), Times.Once);
    }

    // Verifies that RenoDX uninstall failures are returned unchanged.
    [Fact]
    public async Task UninstallRenoDXUseCase_ExecuteAsync_WhenServiceFails_ReturnsFailureResult()
    {
        var game = new Game { Name = "Game" };
        var expected = new ModOperationResultDto(false, game, "Failed", ShouldPromptForDeepScan: true);
        var modInstall = new Mock<IModInstallService>(MockBehavior.Strict);
        modInstall.Setup(x => x.UninstallRenoDXAsync(game)).ReturnsAsync(expected);
        var sut = new UninstallRenoDXUseCase(modInstall.Object);

        var result = await sut.ExecuteAsync(game);

        Assert.Same(expected, result);
        Assert.False(result.IsSuccess);
        Assert.True(result.ShouldPromptForDeepScan);
    }
}
