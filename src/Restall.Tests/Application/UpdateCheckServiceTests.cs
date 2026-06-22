using Moq;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Services;
using Restall.Domain.Entities;

namespace Restall.Tests.Application;

public sealed class UpdateCheckServiceTests
{
    [Fact]
    public void CheckReShadeUpdate_WhenLatestVersionIsNewer_ReturnsUpdateAvailable()
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestReShadeVersion(ReShade.Branch.Stable)).Returns("6.5.0");
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckReShadeUpdate(new ReShade
        {
            BranchName = ReShade.Branch.Stable,
            Version = "6.4.0"
        });

        Assert.True(result.UpdateAvailable);
        Assert.Equal("6.4.0", result.InstalledVersion);
        Assert.Equal("6.5.0", result.LatestVersion);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData("6.5.0", "6.5.0")]
    [InlineData("6.6.0", "6.5.0")]
    public void CheckReShadeUpdate_WhenLatestVersionIsNotNewer_ReturnsNoUpdate(
        string installedVersion,
        string latestVersion)
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestReShadeVersion(ReShade.Branch.Stable)).Returns(latestVersion);
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckReShadeUpdate(new ReShade
        {
            BranchName = ReShade.Branch.Stable,
            Version = installedVersion
        });

        Assert.False(result.UpdateAvailable);
        Assert.Equal(installedVersion, result.InstalledVersion);
        Assert.Equal(latestVersion, result.LatestVersion);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void CheckReShadeUpdate_WhenBranchIsUnknown_UsesStableBranch()
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestReShadeVersion(ReShade.Branch.Stable)).Returns("6.5.0");
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckReShadeUpdate(new ReShade
        {
            BranchName = ReShade.Branch.Unknown,
            Version = "6.4.0"
        });

        Assert.True(result.UpdateAvailable);
        catalog.Verify(c => c.GetLatestReShadeVersion(ReShade.Branch.Stable), Times.Once);
    }

    [Theory]
    [InlineData(null, "6.5.0")]
    [InlineData("", "6.5.0")]
    [InlineData(" ", "6.5.0")]
    [InlineData("6.4.0", null)]
    [InlineData("6.4.0", "")]
    [InlineData("6.4.0", " ")]
    public void CheckReShadeUpdate_WhenInstalledOrLatestVersionIsMissing_ReturnsNoUpdate(
        string? installedVersion,
        string? latestVersion)
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestReShadeVersion(ReShade.Branch.Stable)).Returns(latestVersion);
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckReShadeUpdate(new ReShade
        {
            BranchName = ReShade.Branch.Stable,
            Version = installedVersion
        });

        Assert.False(result.UpdateAvailable);
        Assert.Equal(installedVersion, result.InstalledVersion);
        Assert.Equal(latestVersion, result.LatestVersion);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData("not-a-version", "6.5.0")]
    [InlineData("6.4.0", "latest")]
    public void CheckReShadeUpdate_WhenVersionCannotBeParsed_ReturnsErrorMessage(
        string installedVersion,
        string latestVersion)
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestReShadeVersion(ReShade.Branch.Stable)).Returns(latestVersion);
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckReShadeUpdate(new ReShade
        {
            BranchName = ReShade.Branch.Stable,
            Version = installedVersion
        });

        Assert.False(result.UpdateAvailable);
        Assert.Equal(installedVersion, result.InstalledVersion);
        Assert.Equal(latestVersion, result.LatestVersion);
        Assert.Contains("Could not get ReShade versions", result.ErrorMessage);
    }

    [Fact]
    public void CheckRenoDXUpdate_WhenLatestSnapshotIsNewer_ReturnsUpdateAvailable()
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot))
            .Returns(Tag(2024, 2, 2, RenoDX.Branch.Snapshot));
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckRenoDXUpdate(new RenoDX
        {
            BranchName = RenoDX.Branch.Snapshot,
            Version = "20240101"
        });

        Assert.True(result.UpdateAvailable);
        Assert.Equal("20240101", result.InstalledVersion);
        Assert.Equal("20240202", result.LatestVersion);
    }

    [Fact]
    public void CheckRenoDXUpdate_WhenLatestSnapshotIsSameDate_ReturnsNoUpdate()
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot))
            .Returns(Tag(2024, 1, 1, RenoDX.Branch.Snapshot));
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckRenoDXUpdate(new RenoDX
        {
            BranchName = RenoDX.Branch.Snapshot,
            Version = "20240101"
        });

        Assert.False(result.UpdateAvailable);
        Assert.Equal("20240101", result.InstalledVersion);
        Assert.Equal("20240101", result.LatestVersion);
    }

    [Fact]
    public void CheckRenoDXUpdate_WhenBranchIsUnknown_UsesSnapshotBranch()
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot))
            .Returns(Tag(2024, 2, 2, RenoDX.Branch.Snapshot));
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckRenoDXUpdate(new RenoDX
        {
            BranchName = RenoDX.Branch.Unknown,
            Version = "20240101"
        });

        Assert.True(result.UpdateAvailable);
        catalog.Verify(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot), Times.Once);
    }

    [Fact]
    public void CheckRenoDXUpdate_WhenBranchIsWiki_UsesSnapshotBranch()
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot))
            .Returns(Tag(2024, 2, 2, RenoDX.Branch.Snapshot));
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckRenoDXUpdate(new RenoDX
        {
            BranchName = RenoDX.Branch.Wiki,
            Version = "20240101"
        });

        Assert.True(result.UpdateAvailable);
        catalog.Verify(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot), Times.Once);
        catalog.Verify(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Wiki), Times.Never);
    }

    [Fact]
    public void CheckRenoDXUpdate_WhenBranchIsNightly_UsesNightlyBranch()
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Nightly))
            .Returns(Tag(2024, 2, 2, RenoDX.Branch.Nightly));
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckRenoDXUpdate(new RenoDX
        {
            BranchName = RenoDX.Branch.Nightly,
            Version = "20240101"
        });

        Assert.True(result.UpdateAvailable);
        catalog.Verify(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Nightly), Times.Once);
    }

    [Fact]
    public void CheckRenoDXUpdate_WhenModIsExternalUnitySource_ReturnsNoUpdateWithoutCatalogLookup()
    {
        var catalog = CreateVersionCatalog();
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckRenoDXUpdate(new RenoDX
        {
            OriginalName = "renodx-unityengine.addon64",
            BranchName = RenoDX.Branch.Snapshot,
            Version = "20240101"
        });

        Assert.False(result.UpdateAvailable);
        Assert.Equal("20240101", result.InstalledVersion);
        Assert.Null(result.LatestVersion);
        catalog.Verify(c => c.GetLatestRenoDXVersionByTag(It.IsAny<RenoDX.Branch>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CheckRenoDXUpdate_WhenInstalledVersionIsMissing_ReturnsNoUpdate(string? installedVersion)
    {
        var catalog = CreateVersionCatalog();
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckRenoDXUpdate(new RenoDX
        {
            BranchName = RenoDX.Branch.Snapshot,
            Version = installedVersion
        });

        Assert.False(result.UpdateAvailable);
        Assert.Null(result.InstalledVersion);
        Assert.Null(result.LatestVersion);
        catalog.Verify(c => c.GetLatestRenoDXVersionByTag(It.IsAny<RenoDX.Branch>()), Times.Never);
    }

    [Fact]
    public void CheckRenoDXUpdate_WhenLatestTagIsMissing_ReturnsNoUpdate()
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot)).Returns((RenoDXTagInfoDto?)null);
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckRenoDXUpdate(new RenoDX
        {
            BranchName = RenoDX.Branch.Snapshot,
            Version = "20240101"
        });

        Assert.False(result.UpdateAvailable);
        Assert.Equal("20240101", result.InstalledVersion);
        Assert.Null(result.LatestVersion);
    }

    [Fact]
    public void CheckRenoDXUpdate_WhenInstalledVersionCannotBeParsed_ReturnsErrorMessage()
    {
        var catalog = CreateVersionCatalog();
        catalog.Setup(c => c.GetLatestRenoDXVersionByTag(RenoDX.Branch.Snapshot))
            .Returns(Tag(2024, 2, 2, RenoDX.Branch.Snapshot));
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckRenoDXUpdate(new RenoDX
        {
            BranchName = RenoDX.Branch.Snapshot,
            Version = "not-a-date"
        });

        Assert.False(result.UpdateAvailable);
        Assert.Equal("not-a-date", result.InstalledVersion);
        Assert.Equal("20240202", result.LatestVersion);
        Assert.Contains("Could not get date from installed RenoDX version", result.ErrorMessage);
    }

    [Theory]
    [InlineData(RenoDX.Branch.Discord)]
    [InlineData(RenoDX.Branch.Nexus)]
    public void CheckRenoDXUpdate_WhenBranchDoesNotSupportAutomatedUpdates_ReturnsNoUpdate(
        RenoDX.Branch branch)
    {
        var catalog = CreateVersionCatalog();
        var service = new UpdateCheckService(catalog.Object);

        var result = service.CheckRenoDXUpdate(new RenoDX
        {
            BranchName = branch,
            Version = "20240101"
        });

        Assert.False(result.UpdateAvailable);
        Assert.Equal("20240101", result.InstalledVersion);
        Assert.Null(result.LatestVersion);
        catalog.Verify(c => c.GetLatestRenoDXVersionByTag(It.IsAny<RenoDX.Branch>()), Times.Never);
    }

    private static Mock<IVersionCatalog> CreateVersionCatalog() => new(MockBehavior.Strict);

    private static RenoDXTagInfoDto Tag(int year, int month, int day, RenoDX.Branch branch) =>
        new(new DateOnly(year, month, day), branch);
}
