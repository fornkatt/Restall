using System.Collections.Immutable;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;

namespace Restall.Infrastructure.Stores;

internal sealed class ModCatalog : IModCatalog
{
    private readonly IParseService _parseService;

    private ImmutableArray<RenoDXModInfoDto> _renoDXWikiMods = [];
    private ImmutableArray<RenoDXGenericModInfoDto> _renoDXGenericWikiMods = [];

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

    public ImmutableArray<RenoDXModInfoDto> GetRenoDXWikiMods() => _renoDXWikiMods;

    public ImmutableArray<RenoDXGenericModInfoDto> GetRenoDXGenericWikiMods() => _renoDXGenericWikiMods;
}