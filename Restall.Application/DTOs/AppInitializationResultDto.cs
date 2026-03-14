using Restall.Domain.Entities;

namespace Restall.Application.DTOs;

public record AppInitializationResultDto(
    IReadOnlyList<GameInitResultDto> Games,
    bool Success,
    string? ErrorMessage = null
    );

public record GameInitResultDto(
    Game Game,
    RenoDXModInfoDto? CompatibleMod,
    RenoDXGenericModInfoDto? CompatibleGenericMod,
    UpdateCheckResultDto? ReShadeUpdateResult = null,
    UpdateCheckResultDto? RenoDXUpdateResult = null
    );