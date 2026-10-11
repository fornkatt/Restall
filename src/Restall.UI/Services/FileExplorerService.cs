// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Restall.UI.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace Restall.UI.Services;

public sealed class FileExplorerService : IFileExplorerService
{
    public async Task OpenFolderAsync(string folder)
    {
        var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow is null) return;
        await mainWindow.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folder));
    }
}
