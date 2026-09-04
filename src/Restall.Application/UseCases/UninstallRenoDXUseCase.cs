using Microsoft.Extensions.Logging;
using Restall.Application.Common;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.Logging;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public sealed class UninstallRenoDXUseCase : IUninstallRenoDXUseCase
{
    private readonly ILogger<UninstallRenoDXUseCase> _logger;
    private readonly IModInstallService _modInstallService;

    public UninstallRenoDXUseCase(
        ILogger<UninstallRenoDXUseCase> logger,
        IModInstallService modInstallService
    )
    {
        _logger = logger;
        _modInstallService = modInstallService;
    }

    public ModOperationResultDto Execute(Game game)
    {
        _logger.ModUninstallationStart("RenoDX", game.Name ?? "Unknown", game.ExecutablePath ?? "Unknown");
        
        var result = _modInstallService.UninstallRenoDX(game);

        if (!result.IsSuccess)
        {
            var gameName = game.Name ?? "Unknown";

            var userMessage = result.ErrorType switch
            {
                ErrorType.PermissionDenied =>
                    $"Permission denied uninstalling RenoDX from {gameName}. Check you app permissions and try again.",
                ErrorType.FileSystemError =>
                    $"Failed to uninstall RenoDX from {gameName}. The disk may be full or the file may be locked (game running?).",
                ErrorType.FileNotFound =>
                    "File not found at expected location. It might have been moved or deleted. Please perform a full rescan.",
                _ => $"Failed to uninstall RenoDX from {gameName}. Check the log for details."
            };
            
            _logger.ModUninstallFailure("RenoDX", gameName, result.ErrorMessage, result.Exception);

            return new ModOperationResultDto(false, game, userMessage);
        }
        
        _logger.ModUninstallationComplete("RenoDX", game.Name ?? "Unknown");

        return new ModOperationResultDto(true, result.Value!, "Successfully uninstalled RenoDX!");
    }
}