using Restall.Application.Common;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IModInstallService
{
    /// <summary>
    /// Installs a ReShade or RenoDX mod. Takes a game entity, RenoDX or ReShade entity as T and a path
    /// to the where the mod file is located.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// </para>
    /// </summary>
    Task<Result<Game>> InstallModAsync<T>(Game game, T modToInstall, string sourcePath) where T: class;
    
    /// <summary>
    /// Uninstalls ReShade from a Game entity.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// </para>
    /// </summary>
    Result<Game> UninstallReShade(Game game);
    
    /// <summary>
    /// Uninstalls RenoDX from a Game entity.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// </para>
    /// </summary>
    Result<Game> UninstallRenoDX(Game game);
    Task<Result<Game>> RemoveAllReShadeFilesAsync(Game game);
    Task<Result<Game>> RemoveAllRenoDXFilesAsync(Game game);
}