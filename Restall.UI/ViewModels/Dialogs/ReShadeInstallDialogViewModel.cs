using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restall.Domain.Entities;
using Restall.UI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Restall.UI.ViewModels.Dialogs;

public partial class ReShadeInstallDialogViewModel : ObservableObject
{
    public ReShadeInstallDialogViewModel(
        IReadOnlyList<string> availableVersions
        )
    {
        AvailableVersions = availableVersions;
        _selectedVersion = availableVersions.FirstOrDefault();
        _selectedFileNameOption = FileNameOptions.FirstOrDefault();
        _selectedExtensionOption = ExtensionOptions.FirstOrDefault();
    }

    public event EventHandler? CloseRequested;

    public IReadOnlyList<string> AvailableVersions { get; }

    public IReadOnlyList<ReShadeFileNameOption> FileNameOptions { get; } =
        ReShade.FullFileName
            .Select(kv => new ReShadeFileNameOption(kv.Key, kv.Value))
            .ToList();

    public IReadOnlyList<ReShadeExtensionOption> ExtensionOptions { get; } =
        ReShade.Extension
            .Select(kv => new ReShadeExtensionOption(kv.Key, kv.Value))
            .ToList();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string? _selectedVersion;

    [ObservableProperty]
    private ReShadeFileNameOption? _selectedFileNameOption;

    [ObservableProperty]
    private ReShadeExtensionOption? _selectedExtensionOption;

    public bool WasConfirmed { get; private set; }

    public bool CanConfirm => SelectedVersion is not null 
                           && SelectedFileNameOption is not null 
                           && SelectedExtensionOption is not null;

    public ReShadeInstallSelectionDto? BuildResult() =>
        CanConfirm
            ? new ReShadeInstallSelectionDto(
                SelectedVersion!,
                SelectedFileNameOption!.Value,
                SelectedExtensionOption!.Value
                )
            : null;

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        WasConfirmed = true;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, EventArgs.Empty);

    public record ReShadeFileNameOption(ReShade.FileName Value, string Display);
    public record ReShadeExtensionOption(ReShade.FileExtension Value, string Display);
}