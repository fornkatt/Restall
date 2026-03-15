using Restall.Application.DTOs;
using Restall.Application.Interfaces;

namespace Restall.Infrastructure.Stores;

public class ModCatalog : IModCatalog
{
    private readonly IParseService _parseService;

    private IReadOnlyList<RenoDXModInfoDto> _renoDXWikiMods = [];
    private IReadOnlyList<RenoDXGenericModInfoDto> _renoDXGenericWikiMods = [];

    public ModCatalog(
        IParseService parseService
        )
    {
        _parseService = parseService;
    }

    public async Task FetchModsAsync()
    {
        var result = await _parseService.FetchRenoDXWikiModsAsync();

        _renoDXWikiMods = result.WikiMods;
        _renoDXGenericWikiMods = result.GenericWikiMods;
    }

    public IReadOnlyList<RenoDXModInfoDto> GetRenoDXWikiMods() => _renoDXWikiMods;

    public IReadOnlyList<RenoDXGenericModInfoDto> GetRenoDXGenericWikiMods() => _renoDXGenericWikiMods;
}