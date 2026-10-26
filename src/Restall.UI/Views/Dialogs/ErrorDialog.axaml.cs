// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Controls;
using Avalonia.Input.Platform;
using Restall.UI.ViewModels.Dialogs;
using System;

namespace Restall.UI.Views.Dialogs;

public sealed partial class ErrorDialog : Window
{
    public ErrorDialog()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not ErrorDialogViewModel vm)
            return;

        vm.CloseRequested += (_, _) => Close();
        vm.CopyDetailsRequested += (_, _) => CopyDetailsToClipboard(vm.Details);
    }

    private async void CopyDetailsToClipboard(string details)
    {
        if (Clipboard is { } clipboard)
            await clipboard.SetTextAsync(details);
    }
}

