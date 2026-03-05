using System.Collections.Generic;
using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public interface IModDetectionService
{
    Task<HashSet<ReShade>?> DetectInstalledReShadeAsync(string executablePath);
    Task<HashSet<RenoDX>?> DetectInstalledRenoDXAsync(string executablePath);
}