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
    /// <see cref="ErrorType.PermissionDenied"/>
    /// <br/>
    /// <see cref="ErrorType.FileSystemError"/>
    /// </para>
    /// </summary>
    internal static Result<StringTable?> GetVersionInfo(string filePath, long maxScanBytes = long.MaxValue)
    {
        try
        {
            if (new FileInfo(filePath).Length > maxScanBytes)
                return Result<StringTable?>.Success(null);

            var pe = new PeFile(filePath);
            return Result<StringTable?>.Success(pe.Resources?.VsVersionInfo?.StringFileInfo.StringTable.FirstOrDefault());
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<StringTable?>.Error("Access denied reading file.", ErrorType.PermissionDenied, ex);
        }
        catch (FileNotFoundException ex)
        {
            return Result<StringTable?>.Error("File not found.", ErrorType.FileSystemError, ex);
        }
        catch (PathTooLongException ex)
        {
            return Result<StringTable?>.Error("File path is too long.", ErrorType.FileSystemError, ex);
        }
        catch (IOException ex)
        {
            return Result<StringTable?>.Error("Failed to read file.", ErrorType.FileSystemError, ex);
        }
        catch (SecurityException ex)
        {
            return Result<StringTable?>.Error("Permission denied.", ErrorType.PermissionDenied, ex);
        }
    }
}