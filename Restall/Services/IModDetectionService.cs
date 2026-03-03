using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public interface IModDetectionService
{
    Task<T> DetectInstalledModAsync<T>(string executablePath, T modToDetect) where T: class;
    // Task<RenoDX> DetectInstalledRenoDXAsync(string executablePath);
}