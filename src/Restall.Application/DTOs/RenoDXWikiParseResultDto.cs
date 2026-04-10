namespace Restall.Application.DTOs;

public record RenoDXWikiParseResultDto(
    IReadOnlyList<RenoDXModInfoDto> WikiMods,
    IReadOnlyList<RenoDXGenericModInfoDto> GenericWikiMods
    );