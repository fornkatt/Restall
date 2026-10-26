// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Controls.ApplicationLifetimes;
using Restall.Application.Interfaces.Driving;
using Restall.UI.Interfaces;
using Restall.UI.ViewModels.Dialogs;
using Restall.UI.Views.Dialogs;
using System;
using System.Threading.Tasks;

namespace Restall.UI.ErrorHandling;

internal sealed class ErrorDialogHost
{
    private const string UnexpectedErrorTitle = "Something went wrong";

    private const string UnexpectedErrorMessage =
        "What you tried didn't finish. You can keep using Restall, you can do a rescan to refresh everything. " +
        "If the window stops updating, please restart Restall.";

    private const string StartupFailureTitle = "Restall couldn't start";
    private const string StartupFailureMessage = "Restall has to close. Please check the log for details.";

    private readonly IOpenWebLinkUseCase _openWebLinkUseCase;
    private readonly IFileExplorerService _fileExplorerService;
    private readonly TimeProvider _timeProvider;
    private readonly string _logFolderPath;

    private ErrorDialogViewModel? _openDialog;
    private Task? _openDialogClosed;

    public ErrorDialogHost(
        IOpenWebLinkUseCase openWebLinkUseCase,
        IFileExplorerService fileExplorerService,
        TimeProvider timeProvider,
        string logFolderPath
    )
    {
        _openWebLinkUseCase = openWebLinkUseCase;
        _fileExplorerService = fileExplorerService;
        _timeProvider = timeProvider;
        _logFolderPath = logFolderPath;
    }

    public Task ShowUnexpectedErrorAsync(Exception exception) =>
        ShowAsync(UnexpectedErrorTitle, UnexpectedErrorMessage, "Close", exception);

    public Task ShowStartupFailureAsync(Exception exception) =>
        ShowAsync(StartupFailureTitle, StartupFailureMessage, "Quit", exception);

    private async Task ShowAsync(string title, string message, string dismissButtonText, Exception exception)
    {
        if (_openDialog is not null && _openDialogClosed is not null)
        {
            _openDialog.AddAdditionalError();
            await _openDialogClosed;
            return;
        }

        var errorDialogVm = new ErrorDialogViewModel(title, message, dismissButtonText,
            ErrorDetails.Build(exception, _logFolderPath, _timeProvider.GetUtcNow()),
            _logFolderPath, _openWebLinkUseCase, _fileExplorerService);
        var dialog = new ErrorDialog { DataContext = errorDialogVm };

        _openDialog = errorDialogVm;
        _openDialogClosed = ShowWithOwnerIfVisibleAsync(dialog);

        try
        {
            await _openDialogClosed;
        }
        finally
        {
            _openDialog = null;
            _openDialogClosed = null;
        }
    }

    private static Task ShowWithOwnerIfVisibleAsync(ErrorDialog dialog)
    {
        var owner = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?
            .MainWindow;

        return owner is { IsVisible: true }
            ? dialog.ShowDialog(owner)
            : ShowWithoutOwnerAsync(dialog);
    }

    private static Task ShowWithoutOwnerAsync(ErrorDialog dialog)
    {
        var closed = new TaskCompletionSource();

        dialog.Closed += (_, _) => closed.TrySetResult();
        dialog.Show();

        return closed.Task;
    }
}
