// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Application.Common.Enums;
using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IDownloadService
{
    /// <summary>
    /// Downloads a file from a URL to the specified destination folder, replacing any existing file.
    /// <br/>
    /// <br/>
    /// Doesn't replace the file until the whole download is complete, utilizing a temporary .part file.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ErrorType.PermissionDenied"/>
    /// <br/>
    /// <see cref="ErrorType.FileSystemError"/>
    /// <br/>
    /// <see cref="ErrorType.NetworkTimeout"/>
    /// <br/>
    /// <see cref="ErrorType.DownloadFailed"/>
    /// </para>
    /// </summary>
    Task<Result> DownloadAsync(Uri url, string destinationPath, IProgress<DownloadProgressReport>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the specified ReShade version from a specific branch.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ErrorType.PermissionDenied"/>
    /// <br/>
    /// <see cref="ErrorType.FileSystemError"/>
    /// <br/>
    /// <see cref="ErrorType.NetworkTimeout"/>
    /// <br/>
    /// <see cref="ErrorType.DownloadFailed"/>
    /// </para>
    /// </summary>
    Task<Result> DownloadReShadeAsync(ReShade.Branch branch, string version,
        IProgress<DownloadProgressReport>? progress = null);

    /// <summary>
    /// Downloads a specified RenoDX version from a specific branch.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ErrorType.PermissionDenied"/>
    /// <br/>
    /// <see cref="ErrorType.FileSystemError"/>
    /// <br/>
    /// <see cref="ErrorType.NetworkTimeout"/>
    /// <br/>
    /// <see cref="ErrorType.DownloadFailed"/>
    /// </para>
    /// </summary>
    Task<Result> DownloadRenoDXAsync(RenoDX.Branch branch, string? addonFileName = null, string? version = null,
        string? wikiSnapshotUrl = null, IProgress<DownloadProgressReport>? progress = null);

    /// <summary>
    /// Downloads a RenoDX mod variant hosted on a separate GitHub from the main RenoDX repo.
    /// Does not support branch selection.
    /// <br/>
    /// <para>
    /// Possible ResultErrors:
    /// <br/>
    /// <see cref="ErrorType.PermissionDenied"/>
    /// <br/>
    /// <see cref="ErrorType.FileSystemError"/>
    /// <br/>
    /// <see cref="ErrorType.NetworkTimeout"/>
    /// <br/>
    /// <see cref="ErrorType.DownloadFailed"/>
    /// </para>
    /// </summary>
    Task<Result> DownloadExternalRenoDXAsync(RenoDXWikiModType renoDxWikiModType, string addonFileName,
        IProgress<DownloadProgressReport>? progress = null);
}
