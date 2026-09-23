// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.DependencyInjection;
using Restall.Application.Facades;
using Restall.Application.Interfaces.Driving;
using Restall.Application.UseCases;

namespace Restall.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services
            .AddSingleton<IModManagementFacade, ModManagementFacade>()
            .AddSingleton<IInstallReShadeUseCase, InstallReShadeUseCase>()
            .AddSingleton<IUninstallReShadeUseCase, UninstallReShadeUseCase>()
            .AddSingleton<IInstallRenoDXUseCase, InstallRenoDXUseCase>()
            .AddSingleton<IUninstallRenoDXUseCase, UninstallRenoDXUseCase>()
            .AddSingleton<IGameRefreshUseCase, GameRefreshUseCase>()
            .AddSingleton<IFullLibraryRefreshUseCase, FullLibraryRefreshUseCase>();
}
