using Restall.Application.DTOs;

namespace Restall.Application.Interfaces;

public interface IParseService
{
    Task<IReadOnlyList<string>> FetchReShadeVersionsAsync();

    Task<RenoDXWikiParseResultDto> FetchRenoDXWikiModsAsync();
    Task<RenoDXTagInfoDto?> FetchRenoDXSnapshotAsync();
    Task<IReadOnlyList<RenoDXTagInfoDto>> FetchRenoDXNightlyTagsAsync();
}