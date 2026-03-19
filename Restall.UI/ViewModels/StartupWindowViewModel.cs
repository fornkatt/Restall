using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Application.UseCases;
using System;
using System.Threading.Tasks;

namespace Restall.UI.ViewModels;

// Extends ObservableObject directly rather than ViewModelBase.
// It doesn't participate in the messenger system, it communicates via an event and is then disposed.
public sealed partial class StartupWindowViewModel : ObservableObject
{
    private readonly IFullRefreshLibraryUseCase _refreshLibrary;
    private readonly ILogService _logService;

    public event Action<RefreshLibraryResultDto>? InitializationCompleted;

    [ObservableProperty]
    private string _statusMessage = "Loading...";

    public StartupWindowViewModel(
        ILogService logService,
        IFullRefreshLibraryUseCase refreshLibrary
        )
    {
        _logService = logService;
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

        foreach (var item in result.Games)
        {
            await _logService.LogInfoAsync(item.CompatibleMod is not null
                ? $"Compatible RenoDX game: {item.Game.Name}"
                : item.CompatibleGenericMod is not null
                    ? $"Compatible generic RenoDX game: {item.Game.Name}"
                    : $"Loaded game: {item.Game.Name}");
        }

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true);

        InitializationCompleted?.Invoke(result);
    }
}