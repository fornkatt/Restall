using Restall.Domain.Entities;

namespace Restall.Application.DTOs;

public record AppInitializationResultDto(
    IReadOnlyList<GameInitResultDto> Games
    );

public record GameInitResultDto(
    Game Game,
    RenoDXModInfoDto? CompatibleMod,
    RenoDXGenericModInfoDto? CompatibleGenericMod
    );