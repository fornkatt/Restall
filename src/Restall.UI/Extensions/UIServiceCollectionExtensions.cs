// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

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
        services
            .AddSingleton<IImageResizeService, ImageResizeService>()
            .AddSingleton<IIconConverterService, IconConverterService>();

        services
            .AddSingleton<IModSelectionDialogService, ModSelectionDialogService>();

        services
            .AddSingleton<GameListViewModel>()
            .AddSingleton<ModViewModel>()
            .AddSingleton<MainWindowViewModel>();

        services
            .AddTransient<StartupWindowViewModel>();

        return services;
    }
}
