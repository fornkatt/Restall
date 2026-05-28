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
    /// <see cref="ErrorType.PermissionDenied"/>
    /// <br/>
    /// <see cref="ErrorType.FileSystemError"/>
    /// <br/>
    /// <see cref="ErrorType.FileNotFound"/>
    /// </para>
    /// </summary>
    Result TryDeleteFile(string filePath, string? verifyOriginalFilename = null);
}