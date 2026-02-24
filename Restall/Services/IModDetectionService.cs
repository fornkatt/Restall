using System.Threading.Tasks;

namespace Restall.Services;

public interface IModDetectionService
{
    Task DetectInstallReShadeAsync();

    Task DetectInstallRenoDXAsync();

}