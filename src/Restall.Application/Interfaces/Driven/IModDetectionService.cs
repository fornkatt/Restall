using Restall.Application.Common;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IModDetectionService
{
    /// <summary>
    /// Detects pre-installed ReShade files in a given executable path. Including original filename, filename on disk and version.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// </para>
    /// </summary>
    Task<Result<HashSet<ReShade>>> DetectInstalledReShadeAsync(string executablePath);
    
    /// <summary>
    /// Detects pre-installed RenoDX files in a given executable path. Including original filename, filename on disk and version.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// </para>
    /// </summary>
    Task<Result<HashSet<RenoDX>>> DetectInstalledRenoDXAsync(string executablePath);
    
    /// <summary>
    /// Get file version info from a RenoDX mod file.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// </para>
    /// </summary>
    Result<string?> GetRenoDXFileVersion(string filePath);
}