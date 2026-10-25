// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restall.Application.Interfaces.Driving;
using Restall.UI.Interfaces;
using System;
using System.Threading.Tasks;

namespace Restall.UI.ViewModels.Dialogs;

internal sealed partial class ErrorDialogViewModel : ObservableObject
{
    private static readonly Uri s_reportIssueUrl =
        new("https://github.com/fornkatt/Restall/issues/new?template=bug_report.md");

    private readonly IOpenWebLinkUseCase _openWebLinkUseCase;
    private readonly IFileExplorerService _fileExplorerService;
    private readonly string _logFolderPath;

    public ErrorDialogViewModel(
        string title,
        string message,
        string dismissButtonText,
        string details,
        string logFolderPath,
        IOpenWebLinkUseCase openWebLinkUseCase,
        IFileExplorerService fileExplorerService
    )
    {
        Title = title;
        Message = message;
        DismissButtonText = dismissButtonText;
        Details = details;

        _logFolderPath = logFolderPath;
        _openWebLinkUseCase = openWebLinkUseCase;
        _fileExplorerService = fileExplorerService;
    }

    public event EventHandler? CloseRequested;
    public event EventHandler? CopyDetailsRequested;

    public string Title { get; }
    public string Message { get; }
    public string DismissButtonText { get; }
    public string Details { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAdditionalErrors))]
    [NotifyPropertyChangedFor(nameof(AdditionalErrorsText))]
    private partial int AdditionalErrorCount { get; set; }

    public bool HasAdditionalErrors => AdditionalErrorCount > 0;

    public string AdditionalErrorsText => AdditionalErrorCount == 1
        ? "1 more error happened. Please check the log for details."
        : $"{AdditionalErrorCount} more errors happened. Please check the log for details.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActionStatus))]
    public partial string? ActionStatus { get; private set; }

    public bool HasActionStatus => ActionStatus is not null;

    public void AddAdditionalError() => AdditionalErrorCount++;

    [RelayCommand]
    private void Dismiss() => CloseRequested?.Invoke(this, EventArgs.Empty);

    // TODO: Switch to OpenLogFolderUseCase + IFolderLauncher once the folder launcher port lands
    [RelayCommand]
    private Task OpenLogFolderAsync() =>
        Task.Run(() => _fileExplorerService.OpenFolderAsync(_logFolderPath));

    [RelayCommand]
    private async Task ReportIssueAsync()
    {
        var response = await Task.Run(() => _openWebLinkUseCase.Execute(s_reportIssueUrl));

        ActionStatus = response.IsSuccess ? null : response.Message;
    }

    [RelayCommand]
    private void CopyDetails() => CopyDetailsRequested?.Invoke(this, EventArgs.Empty);
}
