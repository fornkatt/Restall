using Microsoft.Extensions.DependencyInjection;
using Restall.Application.Interfaces;
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
        services.AddTransient<IUpdateModService, UpdateModService>();
        services.AddTransient<IFileExtractionService, FileExtractionService>();
        services.AddTransient<IModDownloadService, ModDownloadService>();

        services.AddHttpClient<IParseService, ParseService>();
        services.AddHttpClient<IModDownloadService, ModDownloadService>();

        return services;
    }
}