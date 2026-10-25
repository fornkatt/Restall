// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

/*
    Restall — ReShade and HDR mod manager
    Copyright (C) 2026  Johan Lager & Kristofer Sell & Filip Klaic

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
    Contact us on our GitHub repository:  https://github.com/fornkatt/Restall
*/

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Restall.Application.Extensions;
using Restall.Infrastructure.Extensions;
using Restall.UI.ErrorHandling;
using Restall.UI.Extensions;
using Restall.UI.ViewModels;
using Restall.UI.Views;
using Serilog;
using System;

namespace Restall.UI;

public class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        TemplatedControl.TemplateAppliedEvent.AddClassHandler<TopLevel>(ApplyGreyscaleTextOptions);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        serviceProvider.GetRequiredService<UnhandledExceptionHandler>().Register();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit += (_, _) => serviceProvider.Dispose();

            var startupVm = serviceProvider.GetRequiredService<StartupWindowViewModel>();
            var startupWindow = new StartupWindow { DataContext = startupVm };

            desktop.MainWindow = startupWindow;

            startupVm.InitializationCompleted += result =>
            {
                var mainWindowVm = serviceProvider.GetRequiredService<MainWindowViewModel>();
                mainWindowVm.GameListViewModel.LoadGames(result);

                var mainWindow = new MainWindow { DataContext = mainWindowVm };
                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                startupWindow.Close();
            };

            RunStartup(startupVm, desktop, serviceProvider.GetRequiredService<ErrorDialogPresenter>());
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ApplyGreyscaleTextOptions(TopLevel topLevel, TemplateAppliedEventArgs e)
    {
        TextOptions.SetTextRenderingMode(topLevel, TextRenderingMode.Antialias);
        TextOptions.SetTextHintingMode(topLevel, TextHintingMode.None);
    }

    private static async void RunStartup(StartupWindowViewModel vm, IClassicDesktopStyleApplicationLifetime desktop,
        ErrorDialogPresenter errorDialogPresenter)
    {
        try
        {
            await vm.InitializeAsync();
        }
        catch (Exception ex)
        {
            Log.ForContext<App>().Fatal(ex, "Fatal error occurred during startup");

            try
            {
                await errorDialogPresenter.ShowStartupFailureAsync(ex);
            }
            finally
            {
                desktop.Shutdown(1);
            }
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices();
        services.AddUIServices();
    }
}
