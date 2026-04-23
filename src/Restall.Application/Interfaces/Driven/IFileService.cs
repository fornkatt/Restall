using Restall.Application.Common;

namespace Restall.Application.Interfaces.Driven;

public interface IFileService
{
    /// <summary>
    /// Tries to delete a file in the given path.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// <br/>
    /// <see cref="ResultError.FileNotFound"/>
    /// </para>
    /// </summary>
    Result TryDeleteFile(string filePath, string? verifyOriginalFilename = null);
}