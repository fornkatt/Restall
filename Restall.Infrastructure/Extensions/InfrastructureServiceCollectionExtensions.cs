using Microsoft.Extensions.DependencyInjection;
using Restall.Application.Facades;
using Restall.Application.Interfaces;
using Restall.Application.Services;
using Restall.Application.UseCases;
using Restall.Infrastructure.Persistence;
using Restall.Infrastructure.Scanners;
using Restall.Infrastructure.Services;
using Restall.Infrastructure.Stores;

namespace Restall.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<ICachePathService, CachePathService>();

        services.AddHttpClient("ParseService");
        services.AddSingleton<IParseService, ParseService>();

        services.AddSingleton<IUpdateCheckService, UpdateCheckService>();
        services.AddSingleton<IVersionCatalog, VersionCatalog>();
        services.AddSingleton<IModCatalog, ModCatalog>();

        services.AddSingleton<ISteamGridDbIndexRepository, SteamGridDbIndexRepository>();
        services.AddSingleton<ISteamGridDbService, SteamGridDbService>();

        services.AddPlatformScanners();
        services.AddSingleton<IEngineDetectionService, EngineDetectionService>();
        services.AddSingleton<IGameDetectionService, GameDetectionService>();
        services.AddSingleton<IModDetectionService, ModDetectionService>();

        services.AddSingleton<IRefreshLibraryUseCase, RefreshLibraryUseCase>();
        
        services.AddTransient<IModInstallService, ModInstallService>();
        services.AddTransient<IFileExtractionService, FileExtractionService>();
        services.AddTransient<IInstallReShadeUseCase, InstallReShadeUseCase>();
        services.AddTransient<IUninstallReShadeUseCase, UninstallReShadeUseCase>();
        services.AddTransient<IInstallRenoDXUseCase, InstallRenoDXUseCase>();
        services.AddTransient<IUninstallRenoDXUseCase, UninstallRenoDXUseCase>();
        services.AddTransient<IModManagementFacade, ModManagementFacade>();

        services.AddHttpClient<IModDownloadService, ModDownloadService>();

        return services;
    }

    private static IServiceCollection AddPlatformScanners(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformScannerService, SteamScanner>();
        services.AddSingleton<IPlatformScannerService, EpicScanner>();
        services.AddSingleton<IPlatformScannerService, GOGScanner>();
        services.AddSingleton<IPlatformScannerService, UbisoftScanner>();
        services.AddSingleton<IPlatformScannerService, EAScanner>();
        return services;
    }
    
}