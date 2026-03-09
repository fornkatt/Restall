using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IModDownloadService
{
    // Branch and version to download, if RenoDX branch of ReShade is requested we just fetch latest. Nightly from GitHub actions artifacts
    Task<bool> DownloadReShadeAsync(ReShade.Branch branch, string version);
    // Branch and version to download, if snapshot or wiki mod we just fetch latest
    Task<bool> DownloadRenoDXAsync(RenoDX.Branch branch, string? addonFileName = null, string? version = null, string? wikiSnapshotUrl = null);
}