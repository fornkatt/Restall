using System.Collections.Immutable;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;

namespace Restall.Infrastructure.Stores;

internal sealed class ModCatalog : IModCatalog
{
    private readonly IParseService _parseService;

    private ImmutableArray<RenoDXModInfoDto> _renoDXWikiMods = [];
    private ImmutableArray<RenoDXGenericModInfoDto> _renoDXGenericWikiMods = [];
    private ImmutableDictionary<ModType, string> _engineNotes = [];

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
        _engineNotes = result.EngineNotes;
    }

    public ImmutableArray<RenoDXModInfoDto> GetRenoDXWikiMods() => _renoDXWikiMods;

    public ImmutableArray<RenoDXGenericModInfoDto> GetRenoDXGenericWikiMods() => _renoDXGenericWikiMods;

    public string? GetModTypeNotes(ModType engine) => _engineNotes.GetValueOrDefault(engine);
}