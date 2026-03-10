using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.UseCases;

public record InstallRenoDXRequest(
    Game Game,
    RenoDXModInfoDto ModInfo,
    RenoDX.Architecture Arch
    );