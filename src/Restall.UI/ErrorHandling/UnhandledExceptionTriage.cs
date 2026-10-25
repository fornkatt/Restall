// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;

namespace Restall.UI.ErrorHandling;

internal sealed class UnhandledExceptionTriage
{
    private const int MaxExceptionsInStormWindow = 3;

    private static readonly TimeSpan s_stormWindow = TimeSpan.FromSeconds(10);

    private readonly TimeProvider _timeProvider;
    private readonly Queue<DateTimeOffset> _recentExceptionTimes = new();

    public UnhandledExceptionTriage(
        TimeProvider timeProvider
    )
    {
        _timeProvider = timeProvider;
    }

    public UnhandledExceptionOutcome Decide(Exception exception)
    {
        if (exception is OperationCanceledException)
            return UnhandledExceptionOutcome.IgnoreCancellation;

        RecordException();

        return exception switch
        {
            OutOfMemoryException => UnhandledExceptionOutcome.ExitApp,
            _ when IsExceptionStorm() => UnhandledExceptionOutcome.ExitApp,
            _ => UnhandledExceptionOutcome.ShowErrorDialog
        };
    }

    private void RecordException()
    {
        var now = _timeProvider.GetUtcNow();

        _recentExceptionTimes.Enqueue(now);

        while (now - _recentExceptionTimes.Peek() > s_stormWindow)
            _recentExceptionTimes.Dequeue();
    }

    private bool IsExceptionStorm() => _recentExceptionTimes.Count > MaxExceptionsInStormWindow;
}
