using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IEngineDetectionService
{
    public (string? executablePath, Game.Engine engine) DetectExecutablePathAndEngine(string rootPath);

}