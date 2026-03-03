using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public class ModDetectionService : IModDetectionService
{
    public async Task<T> DetectInstalledModAsync<T>(string executablePath, T modToDetect) where T: class
    {
        
    }

    private HashSet<ReShade> FindReShadeFiles(string executablePath)
    {
        var files = Directory.GetFiles(executablePath, "*.dll")
            .Concat(Directory.GetFiles(executablePath, "*.asi"))
            .ToArray();
    }

    private HashSet<RenoDX> FindRenoDXFiles(string executablePath)
    {
        throw new System.NotImplementedException();
    }

    // public async Task DetectInstalledRenoDXAsync()
    // {
    //     throw new System.NotImplementedException();
    // }
}