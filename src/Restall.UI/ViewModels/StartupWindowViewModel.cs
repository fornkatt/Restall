using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Application.DTOs;
using Restall.Application.Interfaces.Driving;
using System;
using System.Threading.Tasks;
using Restall.Application.DTOs.Results;

namespace Restall.UI.ViewModels;

// Extends ObservableObject directly rather than ViewModelBase.
// It doesn't participate in the messenger system, it communicates via an event and is then disposed.
public sealed partial class StartupWindowViewModel : ObservableObject
{
    private readonly IRefreshLibraryUseCase _refreshLibrary;

    public event Action<RefreshLibraryResultDto>? InitializationCompleted;

    [ObservableProperty] private string _statusMessage = "Loading...";

    public StartupWindowViewModel(
        IRefreshLibraryUseCase refreshLibrary
    )
    {
        _refreshLibrary = refreshLibrary;
    }

    public async Task InitializeAsync()
    {
        var progress = new Progress<GameScanProgressReportDto>(report =>
        {
            StatusMessage = $"Scanning... Completed: {report.CompletedPlatform} " +
                            $"({report.ScannersCompleted}/{report.TotalScanners})";
        });

        StatusMessage = "Scanning for games...";

        var result = await _refreshLibrary.ExecuteFullRescanAsync(progress);

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true);

        InitializationCompleted?.Invoke(result);
    }
}