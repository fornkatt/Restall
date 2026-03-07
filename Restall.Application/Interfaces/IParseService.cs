using Restall.Application.DTOs;

namespace Restall.Application.Interfaces;

public interface IParseService
{
    Task FetchAvailableModVersionsAsync();
    RenoDXModInfoDto? GetCompatibleRenoDXMod(string? gameName);
}