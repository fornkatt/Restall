using Restall.Application.Common;
using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IModDownloadService
{
    /// <summary>
    /// Downloads the specified ReShade version from a specific branch.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// <br/>
    /// <see cref="ResultError.NetworkTimeout"/>
    /// <br/>
    /// <see cref="ResultError.DownloadFailed"/>
    /// </para>
    /// </summary>
    Task<Result> DownloadReShadeAsync(ReShade.Branch branch, string version, IProgress<DownloadProgressReportDto>? progress = null);
    
    /// <summary>
    /// Downloads a specified RenoDX version from a specific branch.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// <br/>
    /// <see cref="ResultError.NetworkTimeout"/>
    /// <br/>
    /// <see cref="ResultError.DownloadFailed"/>
    /// </para>
    /// </summary>
    Task<Result> DownloadRenoDXAsync(RenoDX.Branch branch, string? addonFileName = null, string? version = null,
        string? wikiSnapshotUrl = null, IProgress<DownloadProgressReportDto>? progress = null);
    
    /// <summary>
    /// Downloads the Unity generic RenoDX mod. There's no branch selection available.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ResultError.PermissionDenied"/>
    /// <br/>
    /// <see cref="ResultError.FileSystemError"/>
    /// <br/>
    /// <see cref="ResultError.NetworkTimeout"/>
    /// <br/>
    /// <see cref="ResultError.DownloadFailed"/>
    /// </para>
    /// </summary>
    Task<Result> DownloadUnityRenoDXAsync(string addonFileName, IProgress<DownloadProgressReportDto>? progress = null);
}