using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Restall.Infrastructure.Extensions;
using Restall.UI.Extensions;
using Restall.UI.ViewModels;
using Restall.UI.Views;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Restall.UI;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        var crashLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Restall", "Logs", $"{DateTime.Now:yyyy-MM-dd}_crash.log");

        // Fall back logging if crash occurs as a last resort during initialization or if LogService cannot be reached.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;

            if (ex is TaskCanceledException or OperationCanceledException)
                return;
            
            Directory.CreateDirectory(Path.GetDirectoryName(crashLogPath)!);
            File.AppendAllText(crashLogPath, $"{DateTime.Now}: {ex}{Environment.NewLine}");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            if (e.Exception.InnerExceptions.All(ex => ex is TaskCanceledException or OperationCanceledException))
            {
                e.SetObserved();
                return;
            }
            
            Directory.CreateDirectory(Path.GetDirectoryName(crashLogPath)!);
            File.AppendAllText(crashLogPath, $"{DateTime.Now:HH:mm:ss} {e.Exception}{Environment.NewLine}");
        };

        
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

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

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}