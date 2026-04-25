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
    /// <see cref="ErrorType.ToolNotFound"/>
    /// <br/>
    /// <see cref="ErrorType.PermissionDenied"/>
    /// <br/>
    /// <see cref="ErrorType.FileSystemError"/>
    /// <br/>
    /// <see cref="ErrorType.ProcessStartFailed"/>
    /// <br/>
    /// <see cref="ErrorType.ExtractionFailed"/>
    /// </para>
    /// </summary>
    Result ExtractFiles(string fileToOpen, string[] filesToExtract, string destinationPath);
}