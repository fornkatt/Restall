using System.Security;
using PeNet;
using PeNet.Header.Resource;
using Restall.Application.Common;

namespace Restall.Infrastructure.Helpers;

internal static class PeVersionHelper
{
    /// <summary>
    /// Get file information through PeNet.<br/>
    /// Used for instance to get the original file name and file version back from a file using PE headers.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// </para>
    /// </summary>
    internal static Result<StringTable?> GetVersionInfo(string filePath, long maxScanBytes = long.MaxValue)
    {
        try
        {
            if (new FileInfo(filePath).Length > maxScanBytes)
                return Result<StringTable?>.Ok(null);

            var pe = new PeFile(filePath);
            return Result<StringTable?>.Ok(pe.Resources?.VsVersionInfo?.StringFileInfo.StringTable.FirstOrDefault());
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<StringTable?>.Err("Access denied reading file.", ResultError.PermissionDenied, ex);
        }
        catch (FileNotFoundException ex)
        {
            return Result<StringTable?>.Err("File not found.", ResultError.FileSystemError, ex);
        }
        catch (PathTooLongException ex)
        {
            return Result<StringTable?>.Err("File path is too long.", ResultError.FileSystemError, ex);
        }
        catch (IOException ex)
        {
            return Result<StringTable?>.Err("Failed to read file.", ResultError.FileSystemError, ex);
        }
        catch (SecurityException ex)
        {
            return Result<StringTable?>.Err("Permission denied.", ResultError.PermissionDenied, ex);
        }
    }
}