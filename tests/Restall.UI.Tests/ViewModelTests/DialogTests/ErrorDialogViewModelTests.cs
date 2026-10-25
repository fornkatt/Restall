// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Restall.UI.Tests.Fakes;
using Restall.UI.ViewModels.Dialogs;

namespace Restall.UI.Tests.ViewModelTests.DialogTests;

public class ErrorDialogViewModelTests
{
    private const string LogFolderPath = "/home/user/.local/share/Restall/Logs";

    private readonly FakeOpenWebLinkUseCase _openWebLinkUseCase = new();
    private readonly FakeFileExplorerService _fileExplorerService = new();
    private readonly ErrorDialogViewModel _sut;

    public ErrorDialogViewModelTests()
    {
        _sut = new ErrorDialogViewModel("Something went wrong", "What you tried didn't finish.",
            "Close", "details", LogFolderPath, _openWebLinkUseCase, _fileExplorerService);
    }

    [Fact]
    public void DismissCommand_Executed_RaisesCloseRequested()
    {
        var closeRequested = false;
        _sut.CloseRequested += (_, _) => closeRequested = true;

        _sut.DismissCommand.Execute(null);

        Assert.True(closeRequested);
    }

    [Fact]
    public async Task OpenLogFolderCommand_Executed_OpensLogFolderWithoutClosing()
    {
        var closeRequested = false;
        _sut.CloseRequested += (_, _) => closeRequested = true;

        await _sut.OpenLogFolderCommand.ExecuteAsync(null);

        Assert.Equal(LogFolderPath, Assert.Single(_fileExplorerService.OpenedFolders));
        Assert.False(closeRequested);
    }

    [Fact]
    public async Task ReportIssueCommand_Executed_OpensBugReportPageWithoutClosing()
    {
        var closeRequested = false;
        _sut.CloseRequested += (_, _) => closeRequested = true;

        await _sut.ReportIssueCommand.ExecuteAsync(null);

        Assert.Equal(new Uri("https://github.com/fornkatt/Restall/issues/new?template=bug_report.md"),
            Assert.Single(_openWebLinkUseCase.OpenedUrls));
        Assert.False(closeRequested);
    }

    [Fact]
    public void CopyDetailsCommand_Executed_RaisesCopyDetailsRequestedWithoutClosing()
    {
        var closeRequested = false;
        var copyDetailsRequested = false;
        _sut.CloseRequested += (_, _) => closeRequested = true;
        _sut.CopyDetailsRequested += (_, _) => copyDetailsRequested = true;

        _sut.CopyDetailsCommand.Execute(null);

        Assert.True(copyDetailsRequested);
        Assert.False(closeRequested);
    }
}
