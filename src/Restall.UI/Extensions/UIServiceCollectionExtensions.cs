using Microsoft.Extensions.DependencyInjection;
using Restall.UI.Interfaces;
using Restall.UI.Services;
using Restall.UI.ViewModels;

namespace Restall.UI.Extensions;

public static class UIServiceCollectionExtensions
{
    /// <summary>
    /// Implementerar Dependency Injection istället för att manuellt implementera
    /// Singletons i varje enskild klass som behöver leva hela appens livslängd och behöver information från samma ställe.
    /// T.ex. hade det varit problematiskt om ModCatalog och VersionCatalog är Transient då alla delar av appen
    /// behöver få ut samma information från dem så det inte blir mismatch.
    ///
    /// Vi har också sett till att inga Transients injiceras i Singletons då de då befordras till Singleton och
    /// det kan få oönskade sidoeffekter.
    /// </summary>
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        services.AddTransient<IModSelectionDialogService, ModSelectionDialogService>();

        services.AddTransient<StartupWindowViewModel>();
        services.AddTransient<BannerViewModel>();
        services.AddTransient<GameListViewModel>();
        services.AddTransient<ModViewModel>();
        services.AddTransient<MainWindowViewModel>();

        return services;
    }
}