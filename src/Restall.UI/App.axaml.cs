// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell
// SPDX-License-Identifier: GPL-3.0-or-later

/*
    Restall — ReShade and HDR mod manager
    Copyright (C) 2026  Johan Lager & Kristofer Sell

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

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Restall.Infrastructure.Extensions;
using Restall.UI.Extensions;
using Restall.UI.ViewModels;
using Restall.UI.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Restall.UI;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Fall back logging if crash occurs as a last resort during initialization
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is TaskCanceledException or OperationCanceledException)
                return;

            Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            if (e.Exception.InnerExceptions.All(ex => ex is TaskCanceledException or OperationCanceledException))
            {
                e.SetObserved();
                return;
            }

            Log.Fatal(e.Exception, "Unobserved task exception");
        };

        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
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

            _ = startupVm.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddInfrastructureServices();
        services.AddUIServices();
    }
}
