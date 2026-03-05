using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IModDetectionService
{
    Task<HashSet<ReShade>?> DetectInstalledReShadeAsync(string executablePath);
    Task<HashSet<RenoDX>?> DetectInstalledRenoDXAsync(string executablePath);
}