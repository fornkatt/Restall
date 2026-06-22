using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Tests.Application;

public sealed class RenoDXDtoTests
{
    // Verifies that RenoDX wiki addon filenames are extracted from x64 and x32 snapshot URLs.
    [Fact]
    public void RenoDXModInfoDto_WhenSnapshotUrlsAreValid_ExtractsAddonFilenames()
    {
        var dto = CreateModInfo(
            snapshot64: "https://example.test/downloads/renodx-game.addon64",
            snapshot32: "https://example.test/downloads/renodx-game.addon32");

        Assert.Equal("renodx-game.addon64", dto.AddonFilename64);
        Assert.Equal("renodx-game.addon32", dto.AddonFilename32);
    }

    // Verifies that invalid or missing snapshot URLs do not produce addon filenames.
    [Fact]
    public void RenoDXModInfoDto_WhenSnapshotUrlsAreInvalid_ReturnsNullAddonFilenames()
    {
        var dto = CreateModInfo(snapshot64: "not a url", snapshot32: null);

        Assert.Null(dto.AddonFilename64);
        Assert.Null(dto.AddonFilename32);
        Assert.False(dto.HasWikiFilename);
    }

    // Verifies that x64-only RenoDX metadata reports x64-only support flags.
    [Fact]
    public void RenoDXModInfoDto_WhenOnlyX64UrlExists_ReportsX64OnlySupport()
    {
        var dto = CreateModInfo(snapshot64: "https://example.test/renodx-game.addon64");

        Assert.True(dto.SupportsX64);
        Assert.False(dto.SupportsX32);
        Assert.False(dto.IsDualArch);
        Assert.True(dto.HasWikiFilename);
    }

    // Verifies that x32-only RenoDX metadata reports x32-only support flags.
    [Fact]
    public void RenoDXModInfoDto_WhenOnlyX32UrlExists_ReportsX32OnlySupport()
    {
        var dto = CreateModInfo(snapshot32: "https://example.test/renodx-game.addon32");

        Assert.False(dto.SupportsX64);
        Assert.True(dto.SupportsX32);
        Assert.False(dto.IsDualArch);
        Assert.True(dto.HasWikiFilename);
    }

    // Verifies that dual-architecture RenoDX metadata reports dual support flags.
    [Fact]
    public void RenoDXModInfoDto_WhenBothSnapshotUrlsExist_ReportsDualArchSupport()
    {
        var dto = CreateModInfo(
            snapshot64: "https://example.test/renodx-game.addon64",
            snapshot32: "https://example.test/renodx-game.addon32");

        Assert.True(dto.SupportsX64);
        Assert.True(dto.SupportsX32);
        Assert.True(dto.IsDualArch);
        Assert.True(dto.HasWikiFilename);
    }

    // Verifies that manual source preference prioritizes Nexus before Discord.
    [Fact]
    public void PreferredManualSource_WhenNexusAndDiscordExist_ReturnsNexus()
    {
        var dto = CreateModInfo(
            discordUrl: "https://discord.test/mod",
            nexusUrl: "https://nexusmods.com/game/mod");

        Assert.Equal(RenoDXModSource.Nexus, dto.PreferredManualSource);
    }

    // Verifies that manual source preference falls back to Discord when Nexus is absent.
    [Fact]
    public void PreferredManualSource_WhenOnlyDiscordExists_ReturnsDiscord()
    {
        var dto = CreateModInfo(discordUrl: "https://discord.test/mod");

        Assert.Equal(RenoDXModSource.Discord, dto.PreferredManualSource);
    }

    // Verifies that manual source preference returns Unknown when no manual source exists.
    [Fact]
    public void PreferredManualSource_WhenNoManualSourceExists_ReturnsUnknown()
    {
        var dto = CreateModInfo();

        Assert.Equal(RenoDXModSource.Unknown, dto.PreferredManualSource);
    }

    // Verifies that generic RenoDX metadata builds Unreal addon names.
    [Fact]
    public void RenoDXGenericModInfoDto_WhenEngineIsUnreal_ReturnsUnrealAddonNames()
    {
        var dto = new RenoDXGenericModInfoDto("Unreal", ":white_check_mark:", SupportedEngine.Unreal);

        Assert.Equal("renodx-unrealengine.addon64", dto.AddonFilename64);
        Assert.Equal("renodx-unrealengine.addon32", dto.AddonFilename32);
    }

    // Verifies that generic RenoDX metadata builds Unity addon names.
    [Fact]
    public void RenoDXGenericModInfoDto_WhenEngineIsUnity_ReturnsUnityAddonNames()
    {
        var dto = new RenoDXGenericModInfoDto("Unity", ":white_check_mark:", SupportedEngine.Unity);

        Assert.Equal("renodx-unityengine.addon64", dto.AddonFilename64);
        Assert.Equal("renodx-unityengine.addon32", dto.AddonFilename32);
    }

    // Verifies that RenoDX tag versions are formatted as yyyyMMdd.
    [Fact]
    public void RenoDXTagInfoDto_Version_FormatsDateAsCompactYearMonthDay()
    {
        var dto = new RenoDXTagInfoDto(new DateOnly(2024, 2, 3), RenoDX.Branch.Snapshot);

        Assert.Equal("20240203", dto.Version);
    }

    private static RenoDXModInfoDto CreateModInfo(
        string? snapshot64 = null,
        string? snapshot32 = null,
        string? discordUrl = null,
        string? nexusUrl = null) =>
        new(
            "Test Game",
            discordUrl,
            snapshot64,
            snapshot32,
            nexusUrl,
            "Maintainer",
            null,
            ":white_check_mark:");
}
