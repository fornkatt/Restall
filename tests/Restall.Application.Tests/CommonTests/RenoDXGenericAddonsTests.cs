// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common;
using Restall.Domain.Common.Enums;

namespace Restall.Application.Tests.CommonTests;

public class RenoDXGenericAddonsTests
{
    [Theory]
    [InlineData("renodx-ue-extended.addon64",
        "https://marat569.github.io/renodx/renodx-ue-extended.addon64")]
    [InlineData("renodx-unityengine.addon64",
        "https://notvoosh.github.io/renodx-unity/renodx-unityengine.addon64")]
    [InlineData("renodx-unityengine.addon32",
        "https://notvoosh.github.io/renodx-unity/renodx-unityengine.addon32")]
    public void GetGenericAddonDownloadUrl_GenericAddonFilename_ReturnsHostUrl(string addonFilename, string expectedUrl)
    {
        var actual = RenoDXGenericAddons.GetGenericAddonDownloadUrl(addonFilename);

        Assert.Equal(new Uri(expectedUrl), actual);
    }

    [Fact]
    public void GetGenericAddonDownloadUrl_GenericAddonFilenameWithOtherCasing_ReturnsHostUrl()
    {
        var actual = RenoDXGenericAddons.GetGenericAddonDownloadUrl("RenoDX-UE-Extended.addon64");

        Assert.Equal(new Uri("https://marat569.github.io/renodx/renodx-ue-extended.addon64"), actual);
    }

    [Fact]
    public void GetGenericAddonDownloadUrl_GameModAddonFilename_ReturnsNull()
    {
        var actual = RenoDXGenericAddons.GetGenericAddonDownloadUrl("renodx-game.addon64");

        Assert.Null(actual);
    }

    [Fact]
    public void GetGenericAddonInfo_UnrealExtendedAddonFilename_ReturnsUnrealExtendedInfoWithEngineIni()
    {
        var actual = RenoDXGenericAddons.GetGenericAddonInfo("renodx-ue-extended.addon64");

        Assert.NotNull(actual);
        Assert.Equal("UE Extended", actual.Name);
        Assert.NotNull(actual.EngineIni);
    }

    [Theory]
    [InlineData("renodx-unityengine.addon64")]
    [InlineData("renodx-unityengine.addon32")]
    public void GetGenericAddonInfo_UnityGenericAddonFilename_ReturnsUnityGenericInfoWithoutEngineIni(string addonFilename)
    {
        var actual = RenoDXGenericAddons.GetGenericAddonInfo(addonFilename);

        Assert.NotNull(actual);
        Assert.Equal("Unity Generic", actual.Name);
        Assert.Null(actual.EngineIni);
    }

    [Fact]
    public void GetGenericAddonInfo_GameModAddonFilename_ReturnsNull()
    {
        var actual = RenoDXGenericAddons.GetGenericAddonInfo("renodx-game.addon64");

        Assert.Null(actual);
    }

    [Fact]
    public void GetUnrealExtendedAddonFilename_64Bit_ReturnsUnrealExtendedFilename()
    {
        var actual = RenoDXGenericAddons.GetUnrealExtendedAddonFilename(Architecture.X64);

        Assert.Equal("renodx-ue-extended.addon64", actual);
    }

    [Fact]
    public void GetUnrealExtendedAddonFilename_32Bit_ReturnsNull()
    {
        var actual = RenoDXGenericAddons.GetUnrealExtendedAddonFilename(Architecture.X32);

        Assert.Null(actual);
    }

    [Theory]
    [InlineData(Architecture.X64, "renodx-unityengine.addon64")]
    [InlineData(Architecture.X32, "renodx-unityengine.addon32")]
    public void GetUnityGenericAddonFilename_Architecture_ReturnsUnityGenericFilename(Architecture architecture,
        string expectedFilename)
    {
        var actual = RenoDXGenericAddons.GetUnityGenericAddonFilename(architecture);

        Assert.Equal(expectedFilename, actual);
    }
}
