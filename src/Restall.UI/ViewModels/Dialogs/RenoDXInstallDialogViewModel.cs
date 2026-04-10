using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restall.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Restall.UI.ViewModels.Dialogs;

public sealed partial class RenoDXInstallDialogViewModel : ObservableObject
{
    public RenoDXInstallDialogViewModel(
        IReadOnlyList<RenoDXTagInfoDto> availableVersions
        )
    {
        AvailableVersions = availableVersions;
        _selectedVersion = availableVersions.FirstOrDefault();
    }

    public event EventHandler? CloseRequested;

    public IReadOnlyList<RenoDXTagInfoDto> AvailableVersions { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CommitNotes))]
    [NotifyPropertyChangedFor(nameof(HasCommitNotes))]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private RenoDXTagInfoDto? _selectedVersion;

    public List<string>? CommitNotes => SelectedVersion?.CommitNotes;
    public bool HasCommitNotes => CommitNotes?.Count > 0;

    public bool CanConfirm => SelectedVersion is not null;
    public bool WasConfirmed { get; private set; }

    public RenoDXTagInfoDto? BuildResult() => CanConfirm ? SelectedVersion : null;

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        WasConfirmed = true;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, EventArgs.Empty);
}