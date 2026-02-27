using System.Threading.Tasks;

namespace Restall.Services;

public interface IModDetectionService
{
    Task DetectInstalledReShadeAsync();
    Task DetectInstalledRenoDXAsync();
}