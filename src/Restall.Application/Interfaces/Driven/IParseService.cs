using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;

namespace Restall.Application.Interfaces.Driven;

public interface IParseService
{
    Task<IReadOnlyList<string>> FetchReShadeVersionsAsync();

    Task<RenoDXWikiParseResultDto> FetchRenoDXWikiModsAsync();
    Task<RenoDXTagInfoDto?> FetchRenoDXSnapshotAsync();
    Task<IReadOnlyList<RenoDXTagInfoDto>> FetchRenoDXNightlyTagsAsync();
}