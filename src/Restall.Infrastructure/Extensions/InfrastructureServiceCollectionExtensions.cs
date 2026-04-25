using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Restall.Application.Facades;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Application.Services;
using Restall.Application.UseCases;
using Restall.Infrastructure.Scanners;
using Restall.Infrastructure.Services;
using Restall.Infrastructure.Stores;


namespace Restall.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IPathService, PathService>();
        services.AddSingleton<ILogService, LogService>();

        services.AddHttpClient("ParseService", c => c.DefaultRequestHeaders.UserAgent.ParseAdd("Restall"));
        services.AddSingleton<IParseService, ParseService>();
        services.AddSingleton<IUpdateCheckService, UpdateCheckService>();
        services.AddSingleton<IVersionCatalog, VersionCatalog>();
        services.AddSingleton<IModCatalog, ModCatalog>();

        services.AddPlatformScanners();
        services.AddSingleton<IEngineDetectionService, EngineDetectionService>();
        services.AddSingleton<IGameDetectionService, GameDetectionService>();
        services.AddSingleton<IModDetectionService, ModDetectionService>();

        services.AddTransient<ILightRefreshLibraryUseCase, RefreshLibraryUseCase>();
        services.AddTransient<IRefreshLibraryUseCase, RefreshLibraryUseCase>();
        services.AddTransient<IModInstallService, ModInstallService>();
        services.AddTransient<IFileExtractionService, FileExtractionService>();
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IInstallReShadeUseCase, InstallReShadeUseCase>();
        services.AddTransient<IUninstallReShadeUseCase, UninstallReShadeUseCase>();
        services.AddTransient<IInstallRenoDXUseCase, InstallRenoDXUseCase>();
        services.AddTransient<IUninstallRenoDXUseCase, UninstallRenoDXUseCase>();
        services.AddTransient<IModManagementFacade, ModManagementFacade>();

        services.AddHttpClient<IModDownloadService, ModDownloadService>();
        services.AddHttpClient<IGameArtworkService, GameArtworkService>(c =>
            {
                c.DefaultRequestHeaders.UserAgent.ParseAdd("Restall/1.0");

            })
            .ConfigurePrimaryHttpMessageHandler(() =>
                OperatingSystem.IsWindows()
                    ? new WinHttpHandler
                        { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }
                    : new SocketsHttpHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate |
                                                 DecompressionMethods.Brotli
                    });
        


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