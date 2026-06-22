using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.Infrastructure.Extensions;
using Restall.Infrastructure.Services;

namespace Restall.Tests.Infrastructure;

public sealed class InfrastructureServiceCollectionExtensionsTests
{
    // Verifies that infrastructure registrations use the expected lifetimes for shared services.
    [Fact]
    public void AddInfrastructureServices_RegistersExpectedSingletonsAndTransients()
    {
        var services = CreateServiceCollection();

        services.AddInfrastructureServices();

        AssertLifetime<IPathService>(services, ServiceLifetime.Singleton);
        AssertLifetime<ILogService>(services, ServiceLifetime.Singleton);
        AssertLifetime<IParseService>(services, ServiceLifetime.Singleton);
        AssertLifetime<IVersionCatalog>(services, ServiceLifetime.Singleton);
        AssertLifetime<IModCatalog>(services, ServiceLifetime.Singleton);
        AssertLifetime<IRefreshLibraryUseCase>(services, ServiceLifetime.Transient);
        AssertLifetime<ILightRefreshLibraryUseCase>(services, ServiceLifetime.Transient);
        AssertLifetime<IModInstallService>(services, ServiceLifetime.Transient);
        AssertLifetime<IFileExtractionService>(services, ServiceLifetime.Transient);
        AssertLifetime<IInstallReShadeUseCase>(services, ServiceLifetime.Transient);
        AssertLifetime<IInstallRenoDXUseCase>(services, ServiceLifetime.Transient);
        AssertLifetime<IUninstallReShadeUseCase>(services, ServiceLifetime.Transient);
        AssertLifetime<IUninstallRenoDXUseCase>(services, ServiceLifetime.Transient);
        AssertLifetime<IModManagementFacade>(services, ServiceLifetime.Transient);
        Assert.Equal(5, services.Count(x => x.ServiceType == typeof(IPlatformScannerService) && x.Lifetime == ServiceLifetime.Singleton));
    }

    // Verifies that the registered service provider can resolve representative services.
    [Fact]
    public void AddInfrastructureServices_BuildsProviderAndResolvesRepresentativeServices()
    {
        var services = CreateServiceCollection();
        services.AddInfrastructureServices();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        Assert.IsType<PathService>(provider.GetRequiredService<IPathService>());
        Assert.Same(provider.GetRequiredService<IVersionCatalog>(), provider.GetRequiredService<IVersionCatalog>());
        Assert.NotSame(provider.GetRequiredService<IFileExtractionService>(), provider.GetRequiredService<IFileExtractionService>());
        Assert.Equal(5, provider.GetServices<IPlatformScannerService>().Count());
        Assert.NotNull(provider.GetRequiredService<IModDownloadService>());
    }

    private static ServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();
        var configuration = new Mock<IConfiguration>(MockBehavior.Loose);
        configuration.Setup(x => x["SteamGridDBApiKey:ApiKey"]).Returns((string?)null);
        services.AddSingleton(configuration.Object);
        return services;
    }

    private static void AssertLifetime<TService>(IServiceCollection services, ServiceLifetime expectedLifetime)
    {
        var descriptor = Assert.Single(services, x => x.ServiceType == typeof(TService));
        Assert.Equal(expectedLifetime, descriptor.Lifetime);
    }
}
