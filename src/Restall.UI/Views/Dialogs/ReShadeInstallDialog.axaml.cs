// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Controls;
using Restall.UI.ViewModels.Dialogs;
using System;

namespace Restall.UI.Views.Dialogs;

public sealed partial class ReShadeInstallDialog : Window
{
    public ReShadeInstallDialog()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ReShadeInstallDialogViewModel vm)
            vm.CloseRequested += (_, _) => Close();
    }
}