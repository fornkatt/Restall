using System.Collections.Immutable;

namespace Restall.Application.DTOs.Results;

public record RenoDXWikiParseResultDto(
    ImmutableArray<RenoDXModInfoDto> WikiMods,
    ImmutableArray<RenoDXGenericModInfoDto> GenericWikiMods
);