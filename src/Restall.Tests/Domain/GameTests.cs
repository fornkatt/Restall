using Restall.Domain.Entities;

namespace Restall.Tests.Domain;

public sealed class GameTests
{
    // Verifies that a game without ReShade is reported as not having ReShade.
    [Fact]
    public void HasReShade_WhenReShadeIsNull_ReturnsFalse()
    {
        var game = new Game();

        Assert.False(game.HasReShade);
    }

    // Verifies that a game with a ReShade record is reported as having ReShade.
    [Fact]
    public void HasReShade_WhenReShadeIsSet_ReturnsTrue()
    {
        var game = new Game { ReShade = new ReShade() };

        Assert.True(game.HasReShade);
    }

    // Verifies that a game without RenoDX is reported as not having RenoDX.
    [Fact]
    public void HasRenoDX_WhenRenoDXIsNull_ReturnsFalse()
    {
        var game = new Game();

        Assert.False(game.HasRenoDX);
    }

    // Verifies that a game with a RenoDX record is reported as having RenoDX.
    [Fact]
    public void HasRenoDX_WhenRenoDXIsSet_ReturnsTrue()
    {
        var game = new Game { RenoDX = new RenoDX() };

        Assert.True(game.HasRenoDX);
    }
}
