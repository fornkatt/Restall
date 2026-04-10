using PeNet;
using PeNet.Header.Resource;

namespace Restall.Infrastructure.Helpers;

internal static class PeVersionHelper
{
    internal static StringTable? GetVersionInfo(string filePath, long maxScanBytes = long.MaxValue)
    {
        if (new FileInfo(filePath).Length > maxScanBytes)
            return null;

        var pe = new PeFile(filePath);
        return pe.Resources?.VsVersionInfo?.StringFileInfo?.StringTable?.FirstOrDefault();
    }
}