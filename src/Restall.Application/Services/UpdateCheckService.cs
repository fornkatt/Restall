using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;

namespace Restall.Application.Services;

public sealed class UpdateCheckService : IUpdateCheckService
{
    private readonly IVersionCatalog _versionCatalog;
    
    private const string s_dateFormat = "yyyyMMdd";

    public UpdateCheckService(
        IVersionCatalog versionCatalog
        )
    {
        _versionCatalog = versionCatalog;
    }

    public UpdateCheckResultDto CheckReShadeUpdate(ReShade installed)
    {
        var branch = installed.BranchName == ReShade.Branch.Unknown
            ? ReShade.Branch.Stable
            : installed.BranchName;

        var installedVersion = installed.Version;
        var latestVersion = _versionCatalog.GetLatestReShadeVersion(branch);

        if (string.IsNullOrWhiteSpace(installedVersion) || string.IsNullOrWhiteSpace(latestVersion))
            return new UpdateCheckResultDto(false, installedVersion, latestVersion);

        if (!Version.TryParse(installedVersion, out var installedSemVer) ||
            !Version.TryParse(latestVersion, out var latestSemVer))
        {
            return new UpdateCheckResultDto(
                false,
                installedVersion,
                latestVersion,
                $"Could not get ReShade versions: Installed = {installedVersion}, Latest = {latestVersion}"
                );
        }

        return new UpdateCheckResultDto(
            latestSemVer > installedSemVer,
            installedVersion,
            latestVersion
            );
    }

    public UpdateCheckResultDto CheckRenoDXUpdate(RenoDX installed)
    {
        if (installed.IsExternalSourceMod)
            return new UpdateCheckResultDto(false, installed.Version, null);

        var branch = installed.BranchName == RenoDX.Branch.Unknown
            ? RenoDX.Branch.Snapshot
            : installed.BranchName;

        var installedVersionString = installed.Version;

        if (string.IsNullOrWhiteSpace(installedVersionString))
            return new UpdateCheckResultDto(false, null, null);

        var effectiveBranch = branch == RenoDX.Branch.Wiki
            ? RenoDX.Branch.Snapshot
            : branch;

        if (effectiveBranch is not (RenoDX.Branch.Snapshot or RenoDX.Branch.Nightly))
            return new UpdateCheckResultDto(false, installedVersionString, null);

        var latestTag = _versionCatalog.GetLatestRenoDXVersionByTag(effectiveBranch);
        if (latestTag is null)
            return new UpdateCheckResultDto(false, installedVersionString, null);

        if (!DateOnly.TryParseExact(installedVersionString, s_dateFormat, null,
            System.Globalization.DateTimeStyles.None, out var installedDate))
            return new UpdateCheckResultDto(
                false,
                installedVersionString,
                latestTag.Version,
                $"Could not get date from installed RenoDX version: {installedVersionString}"
                );

        return new UpdateCheckResultDto(
            latestTag.Date > installedDate,
            installedVersionString,
            latestTag.Version
            );
    }
}