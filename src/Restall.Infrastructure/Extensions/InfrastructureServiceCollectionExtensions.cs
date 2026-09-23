// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.DependencyInjection;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Services;
using Restall.Infrastructure.Scanners;
using Restall.Infrastructure.Services;
using Restall.Infrastructure.Stores;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates;
using System.Net;


namespace Restall.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.ConfigureLogging();

        services.AddSingleton<IUpdateCheckService, UpdateCheckService>();

        services
            .AddSingleton<IVersionCatalog, VersionCatalog>()
            .AddSingleton<IModCatalog, ModCatalog>();

        services
            .AddPlatformScanners()
            .AddSingleton<IEngineDetectionService, EngineDetectionService>()
            .AddSingleton<IGameDetectionService, GameDetectionService>()
            .AddSingleton<IModDetectionService, ModDetectionService>();

        services
            .AddSingleton<IGameIconService, GameIconService>()
            .AddSingleton<IGameArtworkService, GameArtworkService>();

        services
            .AddSingleton<IModInstallService, ModInstallService>()
            .AddSingleton<IFileExtractionService, FileExtractionService>()
            .AddSingleton<IFileService, FileService>();

        services.AddHttpClient(ParseService.HttpClientName, c => c.DefaultRequestHeaders.UserAgent
            .ParseAdd("Restall"));
        services.AddSingleton<IParseService, ParseService>();

        services.AddHttpClient(ModDownloadService.HttpClientName, c => c.DefaultRequestHeaders
            .UserAgent.ParseAdd("Restall"));
        services.AddSingleton<IModDownloadService, ModDownloadService>();

        services.AddHttpClient(GameCoverService.HttpClientName, c => c.DefaultRequestHeaders
                .UserAgent.ParseAdd("Restall"))
            .ConfigurePrimaryHttpMessageHandler(() =>
                OperatingSystem.IsWindows()
                    ? new WinHttpHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    }
                    : new SocketsHttpHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate |
                                                 DecompressionMethods.Brotli
                    });
        services.AddSingleton<IGameCoverService, GameCoverService>();


        return services;
    }

    private static IServiceCollection AddPlatformScanners(this IServiceCollection services) =>
        services
            .AddSingleton<IPlatformScannerService, SteamScanner>()
            .AddSingleton<IPlatformScannerService, EpicScanner>()
            .AddSingleton<IPlatformScannerService, GOGScanner>()
            .AddSingleton<IPlatformScannerService, UbisoftScanner>()
            .AddSingleton<IPlatformScannerService, EAScanner>()
            .AddSingleton<IPlatformScannerService, XboxScanner>();

    private static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        var pathService = new PathService();
        // TODO(logging-refactor): change to Information once settings page lands
        var logLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Debug);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(logLevelSwitch)
            .MinimumLevel.Override("Microsoft.Extensions.Http", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Information)
            .Enrich.WithComputed("ShortContext", "Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)")
            .WriteTo.Async(a => a.File(
                new ExpressionTemplate(
                    "[{@t:HH:mm:ss.fff}] [{@l:u3}] [{ShortContext,-22}]{#if IsDefined(EventId)} [{EventId.Id,4}]{#end} {@m}\n{@x}"),
                Path.Combine(pathService.GetDefaultLogPath(), "restall-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10))
            .CreateLogger();


        services.AddSingleton<IPathService>(pathService);
        services.AddSingleton(logLevelSwitch);
        services.AddLogging(b => b.AddSerilog(dispose: true));

        return services;
    }
}
