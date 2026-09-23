// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Application.Interfaces.Driving;
using System;
using System.Threading.Tasks;

namespace Restall.UI.ViewModels;

// Extends ObservableObject directly rather than ViewModelBase.
// It doesn't participate in the messenger system, it communicates via an event and is then disposed.
public sealed partial class StartupWindowViewModel : ObservableObject
{
    private readonly IFullLibraryRefreshUseCase _fullLibraryRefresh;

    public event Action<RefreshLibraryResultDto>? InitializationCompleted;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Loading...";

    public StartupWindowViewModel(
        IFullLibraryRefreshUseCase fullLibraryRefresh
    )
    {
        _fullLibraryRefresh = fullLibraryRefresh;
    }

    public async Task InitializeAsync()
    {
        var progress = new Progress<GameScanProgressReportDto>(report =>
        {
            StatusMessage = $"Scanning... Completed: {report.CompletedPlatform} " +
                            $"({report.ScannersCompleted}/{report.TotalScanners})";
        });

        StatusMessage = "Scanning for games...";

        var result = await _fullLibraryRefresh.ExecuteAsync(progress);

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true);

        InitializationCompleted?.Invoke(result);
    }
}
