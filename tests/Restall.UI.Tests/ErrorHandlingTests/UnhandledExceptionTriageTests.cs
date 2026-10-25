// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.UI.ErrorHandling;
using Restall.UI.Tests.Fakes;

namespace Restall.UI.Tests.ErrorHandlingTests;

public class UnhandledExceptionTriageTests
{
    private readonly FakeTimeProvider _timeProvider = new();

    private readonly UnhandledExceptionTriage _sut;

    public UnhandledExceptionTriageTests()
    {
        _sut = new UnhandledExceptionTriage(_timeProvider);
    }

    public static TheoryData<Exception> Cancellations => new()
    {
        new OperationCanceledException(), new TaskCanceledException()
    };

    [Theory]
    [MemberData(nameof(Cancellations))]
    public void Decide_Cancellation_ReturnsIgnoreCancellation(Exception cancellation)
    {
        var actual = _sut.Decide(cancellation);

        Assert.Equal(UnhandledExceptionOutcome.IgnoreCancellation, actual);
    }

    [Fact]
    public void Decide_OutOfMemoryException_ReturnsExitApp()
    {
        var actual = _sut.Decide(new OutOfMemoryException());

        Assert.Equal(UnhandledExceptionOutcome.ExitApp, actual);
    }

    [Fact]
    public void Decide_FourthExceptionWithinTenSeconds_ReturnsExitApp()
    {
        var actual = new List<UnhandledExceptionOutcome>();

        for (var i = 0; i < 4; i++)
        {
            actual.Add(_sut.Decide(new InvalidOperationException()));
            _timeProvider.Advance(TimeSpan.FromSeconds(3));
        }

        Assert.Equal([
            UnhandledExceptionOutcome.ShowErrorDialog,
            UnhandledExceptionOutcome.ShowErrorDialog,
            UnhandledExceptionOutcome.ShowErrorDialog,
            UnhandledExceptionOutcome.ExitApp
        ], actual);
    }

    [Fact]
    public void Decide_ExceptionsSpreadOverMoreThanTenSeconds_ReturnsShowErrorDialog()
    {
        var actual = new List<UnhandledExceptionOutcome>();

        for (var i = 0; i < 4; i++)
        {
            actual.Add(_sut.Decide(new InvalidOperationException()));
            _timeProvider.Advance(TimeSpan.FromSeconds(4));

            Assert.All(actual, outcome => Assert.Equal(UnhandledExceptionOutcome.ShowErrorDialog, outcome));
        }
    }

    [Fact]
    public void Decide_WallClockMovedBackwardsAfterStormWindow_ReturnsShowDialog()
    {
        for (var i = 0; i < 3; i++)
            _sut.Decide(new InvalidOperationException());
        _timeProvider.Advance(TimeSpan.FromSeconds(11));
        _timeProvider.MoveWallClockBackwards(TimeSpan.FromHours(1));

        var actual = _sut.Decide(new InvalidOperationException());

        Assert.Equal(UnhandledExceptionOutcome.ShowErrorDialog, actual);
    }

    [Fact]
    public void Decide_ExceptionAfterManyCancellations_ReturnsShowErrorDialog()
    {
        for (var i = 0; i < 5; i++)
            _sut.Decide(new OperationCanceledException());

        var actual = _sut.Decide(new InvalidOperationException());

        Assert.Equal(UnhandledExceptionOutcome.ShowErrorDialog, actual);
    }

    [Fact]
    public void Decide_WallClockMovedBackwardsAfterStormWindow_ReturnsShowErrorDialog()
    {
        for (var i = 0; i < 3; i++)
            _sut.Decide(new InvalidOperationException());
        _timeProvider.Advance(TimeSpan.FromSeconds(11));
        _timeProvider.MoveWallClockBackwards(TimeSpan.FromHours(1));

        var actual = _sut.Decide(new InvalidOperationException());

        Assert.Equal(UnhandledExceptionOutcome.ShowErrorDialog, actual);
    }
}
