// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Restall.UI.Interfaces;
using System.Linq;
using System.Threading.Tasks;
namespace Restall.UI.Services;

public sealed class FolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync()
    {
        var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow is null) return null;
        var folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select the folder that contains your custom games",
            AllowMultiple = false
        });
        return folders.FirstOrDefault()?.TryGetLocalPath();
    }

}
