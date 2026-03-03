using System.Collections.Generic;
using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public interface IModDetectionService
{
    Task<HashSet<ReShade>> FindReShadeFiles(string executablePath);
    // Task<HashSet<T>?> DetectInstalledModAsync<T>(string executablePath, T modToDetect) where T: class;
    // Task<RenoDX> DetectInstalledRenoDXAsync(string executablePath);
}