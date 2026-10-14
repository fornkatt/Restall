// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.Application.Common.Enums;
using Restall.Application.DTOs.RenoDXDTOs;
using Restall.Application.Helpers;
using Restall.Domain.Common.Enums;
using Restall.Domain.Entities;

namespace Restall.Application.Tests.HelperTests;

public class ArchitectureRecommenderTests
{
    private static readonly RenoDXGameMod s_gameModWith64BitFileOnly =
        new("Test Game", RenoDXModStatus.Done, null,
            "https://restalltests.com/renodx-game.addon64", null, null, null,
            null, null);

    [Fact]
    public void Recommend_ReShadeAndRenoDXInstalled_ReturnsReShadeArchitecture()
    {
        var installedReShade = new ReShade { Arch = Architecture.X32 };
        var installedRenoDX = new RenoDX { Arch = Architecture.X64 };
        var match = CreateGameModMatch(s_gameModWith64BitFileOnly);

        var actual = ArchitectureRecommender.Recommend(installedReShade, installedRenoDX, match);

        Assert.Equal(Architecture.X32, actual);
    }

    [Fact]
    public void Recommend_OnlyRenoDXInstalled_ReturnsRenoDXArchitecture()
    {
        var installedRenoDX = new RenoDX { Arch = Architecture.X32 };
        var match = CreateGameModMatch(s_gameModWith64BitFileOnly);

        var actual = ArchitectureRecommender.Recommend(null, installedRenoDX, match);

        Assert.Equal(Architecture.X32, actual);
    }

    [Fact]
    public void Recommend_GameModWith32BitFileOnly_ReturnsX32()
    {
        var gameMod = s_gameModWith64BitFileOnly with
        {
            SnapshotUrl = null,
            SnapshotUrl32 = "https://restalltests.com/renodx-game.addon32"
        };

        var actual = ArchitectureRecommender.Recommend(null, null,
            CreateGameModMatch(gameMod));

        Assert.Equal(Architecture.X32, actual);
    }

    [Fact]
    public void Recommend_GameModWithBothFiles_ReturnsX64()
    {
        var gameMod = s_gameModWith64BitFileOnly with
        {
            SnapshotUrl32 = "https://restalltests.com/renodx-game.addon32"
        };

        var actual = ArchitectureRecommender.Recommend(null, null,
            CreateGameModMatch(gameMod));

        Assert.Equal(Architecture.X64, actual);
    }

    [Fact]
    public void Recommend_UnityGenericModMentions32Bit_ReturnsX32()
    {
        var unityGenericMod = new RenoDXUnityGenericMod("Test Game", RenoDXModStatus.Done, null,
            "32-bit");
        var match = new RenoDXModMatch(null, null, unityGenericMod, RenoDXModMatch.MatchKind.Exact,
            []);

        var actual = ArchitectureRecommender.Recommend(null, null, match);

        Assert.Equal(Architecture.X32, actual);
    }

    // Unreal generic is 64 bit only so it should not get recommended 32 bit
    [Fact]
    public void Recommend_UnrealGenericModMentions32Bit_ReturnsX64()
    {
        var unrealGenericMod = new RenoDXUnrealGenericMod("Test Game", RenoDXModStatus.Done,
            RenoDXUnrealGenericMod.UnrealModMethod.Native, null, "32-bit");
        var match = new RenoDXModMatch(null, unrealGenericMod, null, RenoDXModMatch.MatchKind.Exact,
            []);

        var actual = ArchitectureRecommender.Recommend(null, null, match);

        Assert.Equal(Architecture.X64, actual);
    }

    [Fact]
    public void Recommend_NothingInstalledAndNoMatch_ReturnsX64()
    {
        var actual = ArchitectureRecommender.Recommend(null, null,
            RenoDXModMatch.None);

        Assert.Equal(Architecture.X64, actual);
    }

    private static RenoDXModMatch CreateGameModMatch(RenoDXGameMod gameMod) =>
        new(gameMod, null, null, RenoDXModMatch.MatchKind.Exact, []);
}
