// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Restall.UI.Tests.Fakes;

public sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);
    private long _timestamp;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public void Advance(TimeSpan duration)
    {
        _utcNow += duration;
        _timestamp += duration.Ticks;
    }

    public void MoveWallClockBackwards(TimeSpan duration) => _utcNow -= duration;

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public override long GetTimestamp() => _timestamp;
}
