using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IVersionCatalog
{
    Task FetchVersionsAsync();

    string? GetLatestReShadeVersion(ReShade.Branch branch);
    IReadOnlyList<string> GetAvailableReShadeVersions(ReShade.Branch branch);

    RenoDXTagInfoDto? GetLatestRenoDXVersionByTag(RenoDX.Branch branch);
    IReadOnlyList<RenoDXTagInfoDto> GetAllRenoDXNightlies();
}