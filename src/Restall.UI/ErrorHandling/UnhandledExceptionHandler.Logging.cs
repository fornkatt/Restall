// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.Logging;
using System;

namespace Restall.UI.ErrorHandling;

// Unhandled Exception Handler Logging — EventId range: 2200 - 2249
internal sealed partial class UnhandledExceptionHandler
{
    [LoggerMessage(EventId = 2200, Level = LogLevel.Error,
        Message = "Unhandled exception on UI thread")]
    private partial void LogUnhandledException(Exception ex);

    [LoggerMessage(EventId = 2201, Level = LogLevel.Debug,
        Message = "A cancellation reached the global exception handler")]
    private partial void LogCancellationReachedHandler(Exception ex);

    [LoggerMessage(EventId = 2202, Level = LogLevel.Critical,
        Message = "Unrecoverable exception on the UI thread (out of memory or exception storm), shutting down")]
    private partial void LogUnrecoverableException(Exception ex);

    [LoggerMessage(EventId = 2203, Level = LogLevel.Critical,
        Message = "The global exception handler failed, shutting down")]
    private partial void LogHandlerFailure(Exception ex);
}
