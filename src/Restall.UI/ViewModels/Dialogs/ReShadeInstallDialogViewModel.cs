// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restall.Domain.Entities;
using Restall.UI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Restall.UI.ViewModels.Dialogs;

public sealed partial class ReShadeInstallDialogViewModel : ObservableObject
{
    public ReShadeInstallDialogViewModel(
        IReadOnlyList<string> availableVersions
    )
    {
        AvailableVersions = availableVersions;
        SelectedVersion = availableVersions.FirstOrDefault();
        SelectedFilenameOption = FileNameOptions.FirstOrDefault();
        SelectedExtensionOption = ExtensionOptions.FirstOrDefault();
        IsVersionExpanded = true;
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
    public partial bool IsVersionExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsFilenameExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsExtensionExpanded { get; set; }

    partial void OnIsVersionExpandedChanged(bool value)
    {
        if (value)
        {
            IsFilenameExpanded = false;
            IsExtensionExpanded = false;
        }
    }

    partial void OnIsFilenameExpandedChanged(bool value)
    {
        if (value)
        {
            IsVersionExpanded = false;
            IsExtensionExpanded = false;
        }
    }

    partial void OnIsExtensionExpandedChanged(bool value)
    {
        if (value)
        {
            IsVersionExpanded = false;
            IsFilenameExpanded = false;
        }
    }

    partial void OnSelectedVersionChanged(string? value)
    {
        if (value is null) return;
        IsVersionExpanded = false;
        IsFilenameExpanded = true;
    }

    partial void OnSelectedFilenameOptionChanged(ReShadeFileNameOption? value)
    {
        if (value is null) return;
        IsFilenameExpanded = false;
        IsExtensionExpanded = true;
    }

    partial void OnSelectedExtensionOptionChanged(ReShadeExtensionOption? value)
    {
        if (value is null) return;
        IsExtensionExpanded = false;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial string? SelectedVersion { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedFilename))]
    public partial ReShadeFileNameOption? SelectedFilenameOption { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedExtension))]
    public partial ReShadeExtensionOption? SelectedExtensionOption { get; set; }

    public string? SelectedFilename => SelectedFilenameOption?.Display;

    public string? SelectedExtension => SelectedExtensionOption?.Display;

    public bool WasConfirmed { get; private set; }

    public bool CanConfirm => SelectedVersion is not null
                              && SelectedFilenameOption is not null
                              && SelectedExtensionOption is not null;

    public ReShadeInstallSelectionDto? BuildResult() =>
        CanConfirm
            ? new ReShadeInstallSelectionDto(
                SelectedVersion!,
                SelectedFilenameOption!.Value,
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

    public record ReShadeFileNameOption(ReShade.Filename Value, string Display);

    public record ReShadeExtensionOption(ReShade.FileExtension Value, string Display);
}
