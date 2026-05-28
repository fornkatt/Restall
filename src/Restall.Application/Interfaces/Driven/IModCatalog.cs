using System.Collections.Immutable;
using Restall.Application.DTOs;

namespace Restall.Application.Interfaces.Driven;

public interface IModCatalog
{
    Task FetchModsAsync();

    ImmutableArray<RenoDXModInfoDto> GetRenoDXWikiMods();
    ImmutableArray<RenoDXGenericModInfoDto> GetRenoDXGenericWikiMods();
}