using System.Collections.Immutable;
using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;

namespace Restall.Application.Interfaces.Driven;

public interface IParseService
{
    Task<ImmutableArray<string>> FetchReShadeVersionsAsync();

    Task<RenoDXWikiParseResultDto> FetchRenoDXWikiModsAsync();
    Task<RenoDXTagInfoDto?> FetchRenoDXSnapshotAsync();
    Task<ImmutableArray<RenoDXTagInfoDto>> FetchRenoDXNightlyTagsAsync();
}