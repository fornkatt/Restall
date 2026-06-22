using System.Net;
using Moq;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Domain.Entities;
using Restall.Infrastructure.Services;
using Restall.Tests.TestUtilities;

namespace Restall.Tests.Infrastructure;

public sealed class ModDownloadServiceTests
{
    // Verifies that ReShade downloads use the expected URL and write the installer cache file.
    [Fact]
    public async Task DownloadReShadeAsync_DownloadsExpectedInstallerToCache()
    {
        using var temp = new TempDirectory();
        var bytes = new byte[] { 1, 2, 3 };
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.BytesResponse(bytes));
        var sut = CreateService(temp, handler);
        var progress = new RecordingProgress();

        var result = await sut.DownloadReShadeAsync(ReShade.Branch.Stable, "6.5.0", progress);

        Assert.True(result);
        Assert.Equal(bytes, File.ReadAllBytes(temp.GetPath("reshade", "Stable", "ReShade_Setup_6.5.0_Addon.exe")));
        Assert.Equal("https://reshade.me/downloads/ReShade_Setup_6.5.0_Addon.exe", handler.RequestUris.Single()!.ToString());
        Assert.Contains(progress.Reports, x => x.Filename == "ReShade_Setup_6.5.0_Addon.exe" && x.PercentComplete == 100);
    }

    // Verifies that Unity RenoDX downloads use the Unity endpoint and Snapshot cache.
    [Fact]
    public async Task DownloadUnityRenoDXAsync_DownloadsUnityAddonToSnapshotCache()
    {
        using var temp = new TempDirectory();
        var bytes = new byte[] { 4, 5, 6 };
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.BytesResponse(bytes));
        var sut = CreateService(temp, handler);

        var result = await sut.DownloadUnityRenoDXAsync("renodx-unityengine.addon64");

        Assert.True(result);
        Assert.Equal(bytes, File.ReadAllBytes(temp.GetPath("renodx", "Snapshot", "renodx-unityengine.addon64")));
        Assert.Equal("https://notvoosh.github.io/renodx-unity/renodx-unityengine.addon64", handler.RequestUris.Single()!.ToString());
    }

    // Verifies that RenoDX automated branches build the expected URLs and cache filenames.
    [Theory]
    [InlineData(RenoDX.Branch.Wiki, null, null, "https://example.test/releases/renodx-game.addon64", "Wiki", "renodx-game.addon64", "https://example.test/releases/renodx-game.addon64")]
    [InlineData(RenoDX.Branch.Snapshot, "renodx-unrealengine.addon64", null, null, "Snapshot", "renodx-unrealengine.addon64", "https://github.com/clshortfuse/renodx/releases/download/snapshot/renodx-unrealengine.addon64")]
    [InlineData(RenoDX.Branch.Nightly, "renodx-unrealengine.addon32", "20240203", null, "Nightly", "renodx-unrealengine.addon32", "https://github.com/clshortfuse/renodx/releases/download/nightly-20240203/renodx-unrealengine.addon32")]
    public async Task DownloadRenoDXAsync_WhenBranchSupportsDownload_DownloadsExpectedFile(
        RenoDX.Branch branch,
        string? addonFileName,
        string? version,
        string? wikiSnapshotUrl,
        string cacheBranch,
        string expectedFileName,
        string expectedUrl)
    {
        using var temp = new TempDirectory();
        var bytes = new byte[] { 7, 8, 9 };
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.BytesResponse(bytes));
        var sut = CreateService(temp, handler);

        var result = await sut.DownloadRenoDXAsync(branch, addonFileName, version, wikiSnapshotUrl);

        Assert.True(result);
        Assert.Equal(bytes, File.ReadAllBytes(temp.GetPath("renodx", cacheBranch, expectedFileName)));
        Assert.Equal(expectedUrl, handler.RequestUris.Single()!.ToString());
    }

    // Verifies that invalid RenoDX branch input returns false before making HTTP requests.
    [Theory]
    [InlineData(RenoDX.Branch.Wiki, null, null, null)]
    [InlineData(RenoDX.Branch.Snapshot, null, null, null)]
    [InlineData(RenoDX.Branch.Nightly, "renodx-unrealengine.addon64", null, null)]
    [InlineData(RenoDX.Branch.Nightly, null, "20240203", null)]
    [InlineData(RenoDX.Branch.Nexus, "renodx-unrealengine.addon64", "20240203", null)]
    public async Task DownloadRenoDXAsync_WhenInputCannotBuildDownload_ReturnsFalseWithoutHttp(
        RenoDX.Branch branch,
        string? addonFileName,
        string? version,
        string? wikiSnapshotUrl)
    {
        using var temp = new TempDirectory();
        var handler = new FakeHttpMessageHandler(_ => throw new InvalidOperationException("HTTP should not be called"));
        var sut = CreateService(temp, handler);

        var result = await sut.DownloadRenoDXAsync(branch, addonFileName, version, wikiSnapshotUrl);

        Assert.False(result);
        Assert.Empty(handler.RequestUris);
    }

    // Verifies that existing cache files skip HTTP and report completed progress.
    [Fact]
    public async Task DownloadReShadeAsync_WhenCacheFileAlreadyExists_SkipsHttpAndReportsComplete()
    {
        using var temp = new TempDirectory();
        temp.CreateFile(Path.Combine("reshade", "Stable", "ReShade_Setup_6.5.0_Addon.exe"), "cached");
        var handler = new FakeHttpMessageHandler(_ => throw new InvalidOperationException("HTTP should not be called"));
        var sut = CreateService(temp, handler);
        var progress = new RecordingProgress();

        var result = await sut.DownloadReShadeAsync(ReShade.Branch.Stable, "6.5.0", progress);

        Assert.True(result);
        Assert.Equal("cached", File.ReadAllText(temp.GetPath("reshade", "Stable", "ReShade_Setup_6.5.0_Addon.exe")));
        Assert.Empty(handler.RequestUris);
        Assert.Contains(progress.Reports, x => x.Filename == "ReShade_Setup_6.5.0_Addon.exe" && x.PercentComplete == 100);
    }

    // Verifies that HTTP failures return false and do not leave a successful cache file.
    [Fact]
    public async Task DownloadReShadeAsync_WhenHttpFails_ReturnsFalse()
    {
        using var temp = new TempDirectory();
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.TextResponse("fail", HttpStatusCode.InternalServerError));
        var sut = CreateService(temp, handler);

        var result = await sut.DownloadReShadeAsync(ReShade.Branch.Stable, "6.5.0");

        Assert.False(result);
        Assert.False(File.Exists(temp.GetPath("reshade", "Stable", "ReShade_Setup_6.5.0_Addon.exe")));
    }

    private static ModDownloadService CreateService(TempDirectory temp, FakeHttpMessageHandler handler)
    {
        var pathService = new Mock<IPathService>(MockBehavior.Strict);
        pathService.Setup(x => x.GetReShadeInstallerFilePath(It.IsAny<ReShade.Branch>(), It.IsAny<string>()))
            .Returns((ReShade.Branch branch, string version) => temp.GetPath("reshade", branch.ToString(), $"ReShade_Setup_{version}_Addon.exe"));
        pathService.Setup(x => x.GetRenoDXDownloadCachePath(It.IsAny<RenoDX.Branch>()))
            .Returns((RenoDX.Branch branch) => temp.GetPath("renodx", branch.ToString()));

        return new ModDownloadService(new HttpClient(handler), pathService.Object, new NoOpLogService());
    }

    private sealed class RecordingProgress : IProgress<DownloadProgressReportDto>
    {
        public List<DownloadProgressReportDto> Reports { get; } = [];

        public void Report(DownloadProgressReportDto value) => Reports.Add(value);
    }
}
