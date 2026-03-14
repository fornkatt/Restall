using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Application.Services;

public class UpdateCheckService : IUpdateCheckService
{
    private readonly ILogService _logService;
    private readonly IParseService _parseService;

    private const string s_nightlyPrefix = "nightly-";
    private const string s_dateFormat = "yyyyMMdd";

    public UpdateCheckService(
        ILogService logService,
        IParseService parseService
        )
    {
        _logService = logService;
        _parseService = parseService;
    }

    public UpdateCheckResultDto CheckReShadeUpdate(ReShade installed)
    {
        var branch = installed.BranchName == ReShade.Branch.Unknown
            ? ReShade.Branch.Stable
            : installed.BranchName;

        var installedVersion = installed.Version;
        var latestVersion = _parseService.GetLatestReShadeVersion(branch);

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

        bool hasNightlyPrefix = installedVersionString.StartsWith(s_nightlyPrefix, StringComparison.OrdinalIgnoreCase);

        if (effectiveBranch == RenoDX.Branch.Nightly && !hasNightlyPrefix)
            return new UpdateCheckResultDto(
                false,
                installedVersionString,
                null,
                $"Branch is Nightly but installed version {installedVersionString} is missing the {s_nightlyPrefix} prefix."
                );

        if (effectiveBranch is RenoDX.Branch.Snapshot && hasNightlyPrefix)
            return new UpdateCheckResultDto(
                false,
                installedVersionString,
                null,
                $"Branch is {branch} but installed version {installedVersionString} unexpectedly has the {s_nightlyPrefix} prefix."
                );

        if (effectiveBranch is not (RenoDX.Branch.Snapshot or RenoDX.Branch.Nightly))
            return new UpdateCheckResultDto(false, installedVersionString, null);

        var latestTag = _parseService.GetLatestRenoDXTag(effectiveBranch);

        if (latestTag is null)
            return new UpdateCheckResultDto(false, installedVersionString, null);

        var datePart = hasNightlyPrefix
            ? installedVersionString[s_nightlyPrefix.Length..]
            : installedVersionString;

        if (!DateOnly.TryParseExact(datePart, s_dateFormat, null,
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