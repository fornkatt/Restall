using Microsoft.Extensions.DependencyInjection;
using Restall.Application.Interfaces;
using Restall.Application.Services;
using Restall.Application.UseCases;
using Restall.Infrastructure.Scanners;
using Restall.Infrastructure.Services;

namespace Restall.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<ICachePathService, CachePathService>();

        services.AddScoped<IPlatformScannerService, SteamScanner>();
        services.AddScoped<IPlatformScannerService, EpicScanner>();
        services.AddScoped<IPlatformScannerService, GOGScanner>();
        services.AddScoped<IPlatformScannerService, UbisoftScanner>();
        services.AddScoped<IPlatformScannerService, EAScanner>();
        
        services.AddTransient<IEngineDetectionService, EngineDetectionService>();
        services.AddTransient<IGameDetectionService, GameDetectionService>();
        services.AddTransient<IModDetectionService, ModDetectionService>();
        services.AddTransient<IModInstallService, ModInstallService>();
        services.AddTransient<IFileExtractionService, FileExtractionService>();

        services.AddTransient<IInstallReShadeUseCase, InstallReShadeUseCase>();
        services.AddTransient<IUninstallReShadeUseCase, UninstallReShadeUseCase>();
        services.AddTransient<IInstallRenoDXUseCase, InstallRenoDXUseCase>();
        services.AddTransient<IUninstallRenoDXUseCase, UninstallRenoDXUseCase>();

        services.AddTransient<IAppInitializationService, AppInitializationService>();

        services.AddTransient<IModManagementFacade, ModManagementFacade>();

        services.AddHttpClient("ParseService");
        services.AddSingleton<IParseService, ParseService>();

        services.AddHttpClient<IModDownloadService, ModDownloadService>();

        return services;
    }
}