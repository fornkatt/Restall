using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases.Requests;

public record InstallRenoDXRequest(
    Game Game,
    RenoDX.Architecture Arch,
    RenoDX.Branch Branch,
    RenoDXModInfoDto? ModInfo = null,
    RenoDXGenericModInfoDto? GenericModInfo = null,
    string? TargetVersion = null
    );