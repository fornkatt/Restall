// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Application.Services;

public sealed class UpdateCheckService : IUpdateCheckService
{
    private readonly IVersionCatalog _versionCatalog;

    private const string DateFormat = "yyyyMMdd";

    public UpdateCheckService(
        IVersionCatalog versionCatalog
    )
    {
        _versionCatalog = versionCatalog;
    }

    public UpdateCheck CheckReShadeUpdate(ReShade installed)
    {
        var branch = installed.BranchName == ReShade.Branch.Unknown
            ? ReShade.Branch.Stable
            : installed.BranchName;

        var installedVersion = installed.Version;
        var latestVersion = _versionCatalog.GetLatestReShadeVersion(branch);

        if (string.IsNullOrWhiteSpace(installedVersion) || string.IsNullOrWhiteSpace(latestVersion))
            return new UpdateCheck(false, installedVersion, latestVersion);

        if (!Version.TryParse(installedVersion, out var installedSemVer) ||
            !Version.TryParse(latestVersion, out var latestSemVer))
        {
            return new UpdateCheck(
                false,
                installedVersion,
                latestVersion);
        }

        return new UpdateCheck(
            latestSemVer > installedSemVer,
            installedVersion,
            latestVersion
        );
    }

    public UpdateCheck CheckRenoDXUpdate(RenoDX installed)
    {
        if (!installed.IsUpdateCheckSupported)
            return new UpdateCheck(false, installed.Version, null);

        var branch = installed.BranchName == RenoDX.Branch.Unknown
            ? RenoDX.Branch.Snapshot
            : installed.BranchName;

        var installedVersionString = installed.Version;

        if (string.IsNullOrWhiteSpace(installedVersionString))
            return new UpdateCheck(false, null, null);

        var effectiveBranch = branch == RenoDX.Branch.Direct
            ? RenoDX.Branch.Snapshot
            : branch;

        if (effectiveBranch is not (RenoDX.Branch.Snapshot or RenoDX.Branch.Nightly))
            return new UpdateCheck(false, installedVersionString, null);

        var latestTag = _versionCatalog.GetLatestRenoDXVersionByTag(effectiveBranch);
        if (latestTag is null)
            return new UpdateCheck(false, installedVersionString, null);

        if (!DateOnly.TryParseExact(installedVersionString, DateFormat, null,
                System.Globalization.DateTimeStyles.None, out var installedDate))
            return new UpdateCheck(
                false,
                installedVersionString,
                latestTag.Version);

        return new UpdateCheck(
            latestTag.Date > installedDate,
            installedVersionString,
            latestTag.Version
        );
    }
}
