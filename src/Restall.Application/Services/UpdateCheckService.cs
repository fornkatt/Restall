// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Application.Services;

public sealed class UpdateCheckService : IUpdateCheckService
{
    private readonly IVersionCatalog _versionCatalog;

    public UpdateCheckService(
        IVersionCatalog versionCatalog
    )
    {
        _versionCatalog = versionCatalog;
    }

    public UpdateAvailability CheckReShadeUpdate(ReShade installed)
    {
        var branch = installed.BranchName == ReShade.Branch.Unknown
            ? ReShade.Branch.Stable
            : installed.BranchName;

        var installedVersion = installed.Version;
        var latestVersion = _versionCatalog.GetLatestReShadeVersion(branch);

        if (string.IsNullOrWhiteSpace(installedVersion) || string.IsNullOrWhiteSpace(latestVersion))
            return new UpdateAvailability(false, installedVersion, latestVersion);

        if (!Version.TryParse(installedVersion, out var installedSemVer) ||
            !Version.TryParse(latestVersion, out var latestSemVer))
        {
            return new UpdateAvailability(
                false,
                installedVersion,
                latestVersion);
        }

        return new UpdateAvailability(
            latestSemVer > installedSemVer,
            installedVersion,
            latestVersion
        );
    }
}
