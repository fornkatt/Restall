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

using Avalonia;
using Restall.Infrastructure.Startup;
using Serilog;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Restall.UI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        SerilogSetup.CreateBootstrapLogger();
        RegisterCrashHandlers();
        Log.ForContext<Program>().Information("Restall starting on {OS} ({Runtime})",
            RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription);
        try
        {
            var exitCode = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            Log.ForContext<Program>().Information("Restall exited with code {ExitCode}", exitCode);
            return exitCode;
        }
        catch (Exception ex)
        {
            Log.ForContext<Program>().Fatal(ex, "Restall terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToDelegate(message =>
                    Log.ForContext("SourceContext", "Avalonia")
                        .Debug("{AvaloniaMessage}", message),
                Avalonia.Logging.LogEventLevel.Warning);

        if (OperatingSystem.IsLinux() && Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is not null)
        {
            builder = builder.UseWayland();
        }

        return builder;
    }

    private static void RegisterCrashHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.ForContext<Program>().Fatal(e.ExceptionObject as Exception, "Unhandled exception");
            Log.CloseAndFlush();
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            if (e.Exception.InnerExceptions.All(ex => ex is TaskCanceledException or OperationCanceledException))
            {
                e.SetObserved();
                return;
            }

            Log.ForContext<Program>().Error(e.Exception, "Unobserved task exception");
        };
    }
}
