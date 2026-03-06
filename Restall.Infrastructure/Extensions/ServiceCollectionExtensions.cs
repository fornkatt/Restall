using Microsoft.Extensions.DependencyInjection;
using Restall.Application.Interfaces;
using Restall.Infrastructure.Services;

namespace Restall.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<ICachePathService, CachePathService>();

        services.AddTransient<IGameDetectionService, GameDetectionService>();
        services.AddTransient<IModDetectionService, ModDetectionService>();
        services.AddTransient<IModInstallService, ModInstallService>();
        services.AddTransient<IUpdateModService, UpdateModService>();
        services.AddTransient<IFileExtractionService, FileExtractionService>();

        services.AddHttpClient<IParseService, ParseService>();

        return services;
    }
}