using Restall.Application.Interfaces;
using Restall.Domain.Entities;

namespace Restall.Infrastructure.Services;

public class ModDownloadService : IModDownloadService
{
    private const string s_reShadeStartUrl = "https://reshade.me/downloads/ReShade_Setup_";
    private const string s_reShadeEndUrl = "_Addon.exe";
    private const string s_renoDxSnapshotDownloadBaseUrl = "https://github.com/clshortfuse/renodx/releases/download/snapshot/";
    private const string s_renoDxNightlyDownloadBaseUrl = "https://github.com/clshortfuse/renodx/releases/download/";

    private readonly HttpClient _httpClient;
    private readonly ICachePathService _cachePathService;
    private readonly ILogService _logService;

    public ModDownloadService(
        HttpClient httpClient,
        ICachePathService cachePathService,
        ILogService logService
        )
    {
        _httpClient = httpClient;
        _cachePathService = cachePathService;
        _logService = logService;
    }

    // We only handle donwloads from GitHub since Nexus and Discord are problematic. We provide links in the UI for manual download.
    // We need the full download URL stored in the DTO for the wiki branch since they are downloaded from the maintainer's GitHub
    public async Task<bool> DownloadRenoDXAsync(RenoDX.Branch branch, string? addonFileName = null, string? version = null, string? wikiSnapshotUrl = null)
    {
        string downloadUrl;
        string fileName;

        switch (branch)
        {
            case RenoDX.Branch.Wiki:
                if (string.IsNullOrWhiteSpace(wikiSnapshotUrl))
                {
                    await _logService.LogWarningAsync("RenoDX wiki branch requires a wiki snapshot URL");
                    return false;
                }
                downloadUrl = wikiSnapshotUrl;
                fileName = Path.GetFileName(new Uri(wikiSnapshotUrl).AbsolutePath);
                break;
            case RenoDX.Branch.Snapshot:
                if (string.IsNullOrWhiteSpace(addonFileName))
                {
                    await _logService.LogWarningAsync("RenoDX snapshot branch requires a filename to download.");
                    return false;
                }
                downloadUrl = $"{s_renoDxSnapshotDownloadBaseUrl}{addonFileName}";
                fileName = addonFileName;
                break;
            case RenoDX.Branch.Nightly:
                if (string.IsNullOrWhiteSpace(addonFileName) || string.IsNullOrWhiteSpace(version))
                {
                    await _logService.LogWarningAsync("RenoDX nightly branch requires both addon filename and version.");
                    return false;
                }
                downloadUrl = $"{s_renoDxNightlyDownloadBaseUrl}{version}/{addonFileName}";
                fileName = addonFileName;
                break;
            default:
                await _logService.LogWarningAsync($"Branch {branch} does not support automated downloads.");
                return false;
        }

        var cacheDir = _cachePathService.GetRenoDXDownloadCachePath(branch);
        return await DownloadFileAsync(downloadUrl, cacheDir, fileName);
    }

    public async Task<bool> DownloadReShadeAsync(ReShade.Branch branch, string version)
    {
        var downloadUrl = $"{s_reShadeStartUrl}{version}{s_reShadeEndUrl}";
        var fileName = $"ReShade_Setup_{version}_Addon.exe";
        var cacheDir = _cachePathService.GetReShadeDownloadCachePath(branch);

        return await DownloadFileAsync (downloadUrl, cacheDir, fileName);
    }

    private async Task<bool> DownloadFileAsync(string url, string directory, string fileName)
    {
        try
        {
            if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

            var destinationPath = Path.Combine(directory, fileName);

            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(destinationPath, FileMode.Create,FileAccess.Write, FileShare.None);
            await contentStream.CopyToAsync(fileStream);

            await _logService.LogInfoAsync($"Successfully downloaded {fileName} to {directory}");
            return true;
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync($"Failed to download {fileName} from {url}", ex);
            return false;
        }
    }
}