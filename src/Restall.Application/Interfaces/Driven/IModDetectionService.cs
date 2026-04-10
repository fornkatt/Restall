using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IModDetectionService
{
    Task<HashSet<ReShade>?> DetectInstalledReShadeAsync(string executablePath);
    Task<HashSet<RenoDX>?> DetectInstalledRenoDXAsync(string executablePath);
    string? GetRenoDXFileVersion(string filePath);
}