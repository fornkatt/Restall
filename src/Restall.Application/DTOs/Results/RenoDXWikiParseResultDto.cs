namespace Restall.Application.DTOs.Results;

public record RenoDXWikiParseResultDto(
    IReadOnlyList<RenoDXModInfoDto> WikiMods,
    IReadOnlyList<RenoDXGenericModInfoDto> GenericWikiMods
);