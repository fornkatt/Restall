using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

public class CachePathService : ICachePathService
{
    private const string s_downloadCacheFolderName = "DownloadCache";
    private const string s_cacheFolderName = "Cache";

    private readonly string _reShadeCacheBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, s_cacheFolderName, "ReShade");
    private readonly string _renoDXCacheBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, s_cacheFolderName, "RenoDX");
    private readonly string _reShadeDownloadCacheBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, s_downloadCacheFolderName, "ReShade");
    private readonly string _renoDXDownloadCacheBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, s_downloadCacheFolderName, "RenoDX");

    public string GetReShadeCachePath(ReShade reShade) =>
        Path.Combine(_reShadeCacheBaseDir, reShade.BranchName.ToString(), reShade.Version!);

    public string GetRenoDXCachePath(RenoDX renoDx) =>
        Path.Combine(_renoDXDownloadCacheBaseDir, renoDx.BranchName.ToString(), renoDx.OriginalName!);

    public string GetReShadeDownloadCachePath(ReShade.Branch branch) =>
        Path.Combine(_reShadeDownloadCacheBaseDir, branch.ToString());

    public string GetRenoDXDownloadCachePath(RenoDX.Branch branch) =>
        Path.Combine(_renoDXDownloadCacheBaseDir, branch.ToString());

    public string GetReShadeInstallerFilePath(ReShade.Branch branch, string version) =>
        Path.Combine(GetReShadeDownloadCachePath(branch), $"ReShade_Setup_{version}_Addon.exe");

    public string GetReShadeExtractedFilePath(ReShade reShade) =>
        Path.Combine(GetReShadeCachePath(reShade), reShade.OriginalFileName);
}