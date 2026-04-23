using Restall.Application.Common;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public sealed class UninstallRenoDXUseCase : IUninstallRenoDXUseCase
{
    private readonly ILogService _logService;
    private readonly IModInstallService _modInstallService;

    public UninstallRenoDXUseCase(
        ILogService logService,
        IModInstallService modInstallService
        )
    {
        _logService = logService;
        _modInstallService = modInstallService;
    }

    public async Task<ModOperationResultDto> ExecuteAsync(Game game)
    {
        var result = _modInstallService.UninstallRenoDX(game);

        if (!result.IsSuccess)
        {
            await _logService.LogErrorAsync(result.ErrorMessage ?? $"Failed to uninstall RenoDX from {game.Name}", result.Exception);

            var userMessage = result.Error switch
            {
                ResultError.PermissionDenied => $"Permission denied uninstalling RenoDX from {game.Name}. Check you app permissions and try again.",
                ResultError.FileSystemError => $"Failed to uninstall RenoDX from {game.Name}. The disk may be full or the file may be locked (game running?).",
                ResultError.FileNotFound => "File not found at expected location. It might have been moved or deleted. Please perform a full rescan.",
                _ => $"Failed to uninstall RenoDX from {game.Name}. Check the log for details."
            };

            return new ModOperationResultDto(false, game, userMessage);
        }
        
        return new ModOperationResultDto(true, result.Value!, "Successfully uninstalled RenoDX!");
    }
}