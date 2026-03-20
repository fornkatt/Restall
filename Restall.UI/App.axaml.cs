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
    /// <summary>
    /// Implementerar Dependency Injection istället för att manuellt implementera
    /// Singletons i varje enskild klass som behöver leva hela appens livslängd och behöver information från samma ställe.
    /// T.ex. hade det varit problematiskt om ModCatalog och VersionCatalog är Transient då alla delar av appen
    /// behöver få ut samma information från dem så det inte blir mismatch.
    ///
    /// Vi har också sett till att inga Transients injiceras i Singletons då de befordras till Singleton och
    /// det kan få oönskade sidoeffekter.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        var crashLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "crash.log");

        // Fall back logging if crash occurs as a last resort during initialization or if LogService cannot be reached.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            File.AppendAllText(crashLogPath, $"{DateTime.Now}: {ex}{Environment.NewLine}");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            File.AppendAllText(crashLogPath, $"{DateTime.Now} {e.Exception}{Environment.NewLine}");
        };

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<App>()
            .Build();
        
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
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