using System.Security;
using PeNet;
using PeNet.Header.Resource;
using Restall.Application.Common;

namespace Restall.Infrastructure.Helpers;

internal static class PeVersionHelper
{
    /// <summary>
    /// Get file information through PeNet.<br/><br/>
    /// Used for instance to get the original file name and file version back from a file using PE headers.
    /// </summary>
    internal static StringTable? GetVersionInfo(string filePath, long maxScanBytes = long.MaxValue)
    {
        if (new FileInfo(filePath).Length > maxScanBytes)
            return null;

        var pe = new PeFile(filePath);

        return pe.Resources?.VsVersionInfo?.StringFileInfo.StringTable.FirstOrDefault();
    }
}