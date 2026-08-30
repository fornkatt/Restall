using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.Logging;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public sealed class UninstallReShadeUseCase : IUninstallReShadeUseCase
{
    private readonly ILogger<UninstallReShadeUseCase> _logger;
    private readonly IModInstallService _modInstallService;

    public UninstallReShadeUseCase(
        ILogger<UninstallReShadeUseCase> logger,
        IModInstallService modInstallService
    )
    {
        _logger = logger;
        _modInstallService = modInstallService;
    }

    public ModOperationResultDto Execute(Game game)
    {
        var result = _modInstallService.UninstallReShade(game);

        if (!result.IsSuccess)
        {
            var gameName = game.Name ?? "Unknown";

            var userMessage = result.ErrorType switch
            {
                ErrorType.PermissionDenied =>
                    $"Permission denied uninstalling ReShade from {gameName}. " +
                    $"Check your app permissions and try again.",
                ErrorType.FileSystemError =>
                    $"Failed to uninstall ReShade from {gameName}. " +
                    $"The disk may be full or the file may be locked (game running?).",
                ErrorType.FileNotFound =>
                    "File not found at expected location. It might have been moved or deleted. " +
                    "Please perform a full rescan.",
                _ => $"Failed to uninstall ReShade from {gameName}. Check the log for details."
            };
            
            _logger.ModUninstallFailure("ReShade", gameName, result.ErrorMessage, result.Exception);

            return new ModOperationResultDto(false, game, userMessage);
        }

        return new ModOperationResultDto(true, result.Value!, "Successfully uninstalled ReShade!");
    }
}