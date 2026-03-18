using Avalonia.Controls;
using Restall.UI.ViewModels.Dialogs;
using System;

namespace Restall.UI.Views.Dialogs;

public sealed partial class RenoDXInstallDialog : Window
{
    public RenoDXInstallDialog()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is RenoDXInstallDialogViewModel vm)
            vm.CloseRequested += (_, _) => Close();
    }
}