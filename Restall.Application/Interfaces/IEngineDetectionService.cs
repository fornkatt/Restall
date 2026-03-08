using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IEngineDetectionService
{
    string? DetectExecutablePathAndEngine(string rootPath, out Game.Engine engine);
}