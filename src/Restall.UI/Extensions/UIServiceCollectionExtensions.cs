// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.Extensions.DependencyInjection;
using Restall.Application.Interfaces.Driven;
using Restall.Application.Interfaces.Driving;
using Restall.UI.ErrorHandling;
using Restall.UI.Interfaces;
using Restall.UI.Services;
using Restall.UI.ViewModels;
using System;

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
            .AddSingleton<IFolderPickerService, FolderPickerService>()
            .AddSingleton<IFileExplorerService, FileExplorerService>();

        // TODO: Once the folder launcher port lands, give ErrorDialogViewModel an OpenLogFolderUseCase instead of
        // the log path + IFileExplorerService
        services
            .AddSingleton(TimeProvider.System)
            .AddSingleton(sp => new ErrorDialogPresenter(
                sp.GetRequiredService<IOpenWebLinkUseCase>(),
                sp.GetRequiredService<IFileExplorerService>(),
                sp.GetRequiredService<TimeProvider>(),
                sp.GetRequiredService<IPathService>().GetDefaultLogPath()))
            .AddSingleton<UnhandledExceptionTriage>()
            .AddSingleton<UnhandledExceptionHandler>();

        services
            .AddSingleton<GameListViewModel>()
            .AddSingleton<ModViewModel>()
            .AddSingleton<MainWindowViewModel>();

        services
            .AddTransient<StartupWindowViewModel>();

        return services;
    }
}
