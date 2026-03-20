using Restall.Application.DTOs;

namespace Restall.Application.Interfaces;

public interface IModCatalog
{
    Task FetchModsAsync();

    IReadOnlyList<RenoDXModInfoDto> GetRenoDXWikiMods();
    IReadOnlyList<RenoDXGenericModInfoDto> GetRenoDXGenericWikiMods();
}