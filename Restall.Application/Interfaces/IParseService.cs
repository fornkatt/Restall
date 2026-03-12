using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IParseService
{
    Task<WikiParseResultDto> FetchAvailableModsAsync();

    // Get latest RenoDX snapshot or nightly
    RenoDXTagInfoDto? GetLatestRenoDXTag(RenoDX.Branch branch);

    IReadOnlyList<RenoDXTagInfoDto> GetAllRenoDXNightlies();
    IReadOnlyList<string> GetAvailableReShadeVersions(ReShade.Branch branch); 

    // Gets latest ReShade version depending on branch. Stable, nightly or RenoDX
    string? GetLatestReShadeVersion(ReShade.Branch branch);
}