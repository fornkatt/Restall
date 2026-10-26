// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Controls;
#if DEBUG
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
#endif

namespace Restall.UI.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.Y, KeyModifiers.Control | KeyModifiers.Shift),
            Command = new AsyncRelayCommand(ThrowTestExceptionAsync)
        });
#endif
    }

#if DEBUG
    private static Task ThrowTestExceptionAsync() => Task.Run(ThrowTestException);

    private static void ThrowTestException() =>
        throw new InvalidOperationException("Test exception from the debug trigger (Ctrl+Shift+Y).");
#endif
}
