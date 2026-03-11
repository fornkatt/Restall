using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public record InstallRenoDXRequest(
    Game Game,
    RenoDX.Architecture Arch,
    RenoDX.Branch Branch,
    RenoDXModInfoDto? ModInfo = null,
    RenoDXGenericModInfoDto? GenericModInfo = null
    );