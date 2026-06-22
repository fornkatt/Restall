using Restall.Domain.Entities;

namespace Restall.Tests.Domain;

public sealed class GameTests
{
    [Fact]
    public void HasReShade_WhenReShadeIsNull_ReturnsFalse()
    {
        var game = new Game();

        Assert.False(game.HasReShade);
    }

    [Fact]
    public void HasReShade_WhenReShadeIsSet_ReturnsTrue()
    {
        var game = new Game { ReShade = new ReShade() };

        Assert.True(game.HasReShade);
    }

    [Fact]
    public void HasRenoDX_WhenRenoDXIsNull_ReturnsFalse()
    {
        var game = new Game();

        Assert.False(game.HasRenoDX);
    }

    [Fact]
    public void HasRenoDX_WhenRenoDXIsSet_ReturnsTrue()
    {
        var game = new Game { RenoDX = new RenoDX() };

        Assert.True(game.HasRenoDX);
    }
}
