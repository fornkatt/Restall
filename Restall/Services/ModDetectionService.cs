using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Restall.Models;

namespace Restall.Services;

public class ModDetectionService(ILogService logService) : IModDetectionService
{
    // public async Task<HashSet<T>?> DetectInstalledModAsync<T>(string executablePath, T modToDetect) where T : class
    // {
    //     return await Task.Run(() =>
    //     {
    //         if (typeof(T) == typeof(ReShade))
    //             return FindReShadeFiles(executablePath) as HashSet<T>;
    //
    //         if (typeof(T) == typeof(RenoDX))
    //             return FindRenoDXFiles(executablePath) as HashSet<T>;
    //
    //         return null;
    //     });
    // }

    public async Task<HashSet<ReShade>> FindReShadeFiles(string executablePath)
    {
        var files = Directory.GetFiles(executablePath, "*.dll")
            .Concat(Directory.GetFiles(executablePath, "*.asi"))
            .ToArray();
        
        var fileList = new HashSet<ReShade>();

        foreach (var file in files)
        {
            try
            {
                var fileInfo = FileVersionInfo.GetVersionInfo(file);

                if ((fileInfo.ProductName != null &&
                     fileInfo.ProductName.Equals("ReShade", StringComparison.InvariantCultureIgnoreCase)) &&
                    fileInfo.OriginalFilename != null)
                {
                    var newReShade = new ReShade()
                    {
                        SelectedFileName = fileInfo.FileName,
                        Version = fileInfo.ProductVersion,
                        Arch = fileInfo.OriginalFilename.Contains("64") ? ReShade.Architecture.x64 : ReShade.Architecture.x32,
                    };
                    
                    fileList.Add(newReShade);
                    await logService.LogInfoAsync($"Found ReShade as: {file}");
                }
            }
            catch (Exception ex)
            {
                await logService.LogErrorAsync($"Failed to read file {file}: ", ex);
            }
        }
        
        return fileList;
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