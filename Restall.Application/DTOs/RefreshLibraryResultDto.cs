using Restall.Domain.Entities;

namespace Restall.Application.DTOs;

public record RefreshLibraryResultDto(
    IReadOnlyList<GameInitResultDto> Games,
    bool IsSuccess,
    string? ErrorMessage = null
    );

public record GameInitResultDto(
    Game Game,
    RenoDXModInfoDto? CompatibleMod,
    RenoDXGenericModInfoDto? CompatibleGenericMod,
    UpdateCheckResultDto? ReShadeUpdateResult = null,
    UpdateCheckResultDto? RenoDXUpdateResult = null
    );