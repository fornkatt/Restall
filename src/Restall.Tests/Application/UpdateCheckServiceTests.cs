using Moq;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Services;
using Restall.Domain.Entities;

namespace Restall.Tests.Application;

public sealed class UpdateCheckServiceTests
{
    // Verifies that ReShade reports an update when the catalog version is newer.
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

    // Verifies that ReShade does not report an update for same or newer installed versions.
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

    // Verifies that unknown ReShade branch records fall back to the Stable catalog branch.
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

    // Verifies that missing ReShade version data returns a safe no-update result.
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

    // Verifies that malformed ReShade versions return a parse error instead of an update.
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

    // Verifies that RenoDX snapshot reports an update when the catalog date is newer.
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

    // Verifies that RenoDX snapshot does not report an update when dates match.
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

    // Verifies that unknown RenoDX branch records fall back to Snapshot.
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

    // Verifies that Wiki-sourced RenoDX records compare against Snapshot releases.
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

    // Verifies that Nightly RenoDX records use the Nightly catalog branch.
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

    // Verifies that external Unity RenoDX mods skip automated update checks.
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

    // Verifies that missing RenoDX installed versions return a safe no-update result.
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

    // Verifies that absent RenoDX catalog data returns no update instead of failing.
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

    // Verifies that malformed RenoDX date versions return a parse error.
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

    // Verifies that manual RenoDX source branches do not perform catalog lookups.
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
