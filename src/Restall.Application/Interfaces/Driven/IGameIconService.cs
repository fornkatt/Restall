namespace Restall.Application.Interfaces.Driven;

public interface IGameIconService
{
    Task ExtractIconIfMissingAsync(string? executablePath, string? gameName, string iconPath);
}