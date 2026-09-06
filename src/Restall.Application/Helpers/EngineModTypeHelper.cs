using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Helpers;

public static class EngineModTypeHelper
{
    public static ModType? GetFallbackModType(Game.Engine engine) => engine switch
    {
        Game.Engine.Unreal => ModType.UnrealExtended,
        Game.Engine.Unity => ModType.Unity,
        _ => null
    };
}