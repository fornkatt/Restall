namespace Restall.Application.DTOs;

public record WikiParseResultDto(
    IReadOnlyList<RenoDXModInfoDto> WikiMods,
    IReadOnlyList<RenoDXGenericModInfoDto> GenericWikiMods
    );