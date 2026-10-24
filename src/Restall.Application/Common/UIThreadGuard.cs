// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics;

namespace Restall.Application.Common;

internal static class UIThreadGuard
{
    private const string CalledOnUIThreadMessage =
        "Use case called on the UI thread. Wrap the call in Task.Run in the ViewModel.";

    [Conditional("DEBUG")]
    internal static void AssertNotOnUIThread() =>
        Debug.Assert(SynchronizationContext.Current is null, CalledOnUIThreadMessage);
}
