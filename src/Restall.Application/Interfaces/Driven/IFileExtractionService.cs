using Restall.Application.Common;

namespace Restall.Application.Interfaces.Driven;

public interface IFileExtractionService
{
    /// <summary>
    /// Extracts files from an archive to a destination path.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.ToolNotFound"/>
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// <br/>
    /// <see cref="ResultError.ProcessStartFailed"/>
    /// <br/>
    /// <see cref="ResultError.ExtractionFailed"/>
    /// </para>
    /// </summary>
    Result ExtractFiles(string fileToOpen, string[] filesToExtract, string destinationPath);
}