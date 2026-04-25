using System.Collections.Immutable;
using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IVersionCatalog
{
    Task FetchVersionsAsync();

    string? GetLatestReShadeVersion(ReShade.Branch branch);
    ImmutableArray<string> GetAvailableReShadeVersions(ReShade.Branch branch);

    RenoDXTagInfoDto? GetLatestRenoDXVersionByTag(RenoDX.Branch branch);
    ImmutableArray<RenoDXTagInfoDto> GetAllRenoDXNightlies();
}