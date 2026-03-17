using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Restall.Infrastructure.Extensions;
using Restall.UI.Extensions;
using Restall.UI.ViewModels;
using Restall.UI.Views;
using System.Linq;

namespace Restall.UI;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
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