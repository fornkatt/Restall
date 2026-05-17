using Microsoft.Extensions.DependencyInjection;
using Restall.Application.Interfaces.Driven;
using Restall.UI.Interfaces;
using Restall.UI.Services;
using Restall.UI.ViewModels;

namespace Restall.UI.Extensions;

public static class UIServiceCollectionExtensions
{
    
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        services.AddSingleton<IImageResizeService, ImageResizeService>();
        services.AddSingleton<IIconConverterService, IconConverterService>();
        services.AddTransient<IModSelectionDialogService, ModSelectionDialogService>();
    
        services.AddTransient<StartupWindowViewModel>();
        services.AddTransient<GameListViewModel>();
        services.AddTransient<ModViewModel>();
        services.AddTransient<MainWindowViewModel>();

        return services;
    }
}