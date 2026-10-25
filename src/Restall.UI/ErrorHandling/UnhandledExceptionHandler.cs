// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Restall.UI.ErrorHandling;

internal sealed partial class UnhandledExceptionHandler
{
    internal const string HandledByDispatcherKey = "Restall.HandledByDispatcher";

    private readonly ILogger<UnhandledExceptionHandler> _logger;
    private readonly UnhandledExceptionTriage _triage;
    private readonly ErrorDialogPresenter _errorDialogPresenter;

    public UnhandledExceptionHandler(
        ILogger<UnhandledExceptionHandler> logger,
        UnhandledExceptionTriage triage,
        ErrorDialogPresenter errorDialogPresenter
    )
    {
        _logger = logger;
        _triage = triage;
        _errorDialogPresenter = errorDialogPresenter;
    }

    public void Register() => Dispatcher.UIThread.UnhandledException += OnUnhandledException;

    private void OnUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var exception = e.Exception;

        try
        {
            exception.Data[HandledByDispatcherKey] = true;

            var outcome = _triage.Decide(exception);

            switch (outcome)
            {
                case UnhandledExceptionOutcome.IgnoreCancellation:
                    LogCancellationReachedHandler(exception);
                    e.Handled = true;
                    break;
                case UnhandledExceptionOutcome.ExitApp:
                    LogUnrecoverableException(exception);
                    break;
                case UnhandledExceptionOutcome.ShowErrorDialog:
                    LogUnhandledException(exception);
                    BreakIfDebuggerAttached();
                    Dispatcher.UIThread.Post(() => ShowErrorDialog(exception), DispatcherPriority.Background);
                    e.Handled = true;
                    break;
                default:
                    throw new UnreachableException($"No handling for unhandled exception outcome \"{outcome}\"");
            }
        }
        catch (Exception handlerFailure)
        {
            LogHandlerFailure(handlerFailure);
        }
    }

    private static void BreakIfDebuggerAttached()
    {
        if (Debugger.IsAttached)
            Debugger.Break();
    }

    private async void ShowErrorDialog(Exception exception) =>
        await _errorDialogPresenter.ShowUnexpectedErrorAsync(exception);
}
