using Restall.Domain.Entities;

namespace Restall.Tests.Domain;

public sealed class RenoDXTests
{
    [Theory]
    [InlineData("renodx-unityengine.addon64")]
    [InlineData("ReNoDX-UnityEngine.addon32")]
    public void IsExternalSourceMod_WhenOriginalNameIsUnityEngineMod_ReturnsTrue(string originalName)
    {
        var renoDX = new RenoDX { OriginalName = originalName };

        Assert.True(renoDX.IsExternalSourceMod);
    }

    [Theory]
    [InlineData("renodx-unrealengine.addon64")]
    [InlineData("renodx-game.addon64")]
    [InlineData("unityengine-renodx.addon64")]
    public void IsExternalSourceMod_WhenOriginalNameIsNotUnityEngineMod_ReturnsFalse(string originalName)
    {
        var renoDX = new RenoDX { OriginalName = originalName };

        Assert.False(renoDX.IsExternalSourceMod);
    }

    [Fact]
    public void IsExternalSourceMod_WhenOriginalNameIsNull_ReturnsFalse()
    {
        var renoDX = new RenoDX { OriginalName = null };

        Assert.False(renoDX.IsExternalSourceMod);
    }
}
