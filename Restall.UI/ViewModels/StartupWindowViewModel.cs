using CommunityToolkit.Mvvm.ComponentModel;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace Restall.UI.ViewModels;

public partial class StartupWindowViewModel : ObservableObject
{
    private readonly IAppInitializationService _appInitializationService;
    private readonly ILogService _logService;

    public event Action<AppInitializationResultDto>? InitializationCompleted;

    [ObservableProperty]
    private string _statusMessage = "Loading...";

    public StartupWindowViewModel(
        IAppInitializationService appInitializationService,
        ILogService logService
        )
    {
        _appInitializationService = appInitializationService;
        _logService = logService;
    }

    public async Task InitializeAsync()
    {
        var progress = new Progress<GameScanProgressReportDto>(report =>
        {
            StatusMessage = $"Scanning... Completed: {report.CompletedPlatform} " +
            $"({report.ScannersCompleted}/{report.TotalScanners})";
        });

        StatusMessage = "Scanning for games...";

        var result = await _appInitializationService.InitializeAsync(progress);

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