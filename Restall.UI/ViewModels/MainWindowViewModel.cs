using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.UI.Messages;
using System;
using System.Threading.Tasks;

namespace Restall.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IRecipient<SelectedGameChangedMessage>
{
    private readonly IAppInitializationService _appInitializationService;
    private readonly ILogService _logService;

    private bool _suppressMessage;

    public BannerViewModel BannerViewModel { get; }
    public GameListViewModel GameListViewModel { get; }
    public ModViewModel ModViewModel { get; }

    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    [ObservableProperty]
    private bool _isInitializing = true;

    [ObservableProperty]
    private string _initializationMessage = "Scanning for games...";

    public bool IsGameSelected => SelectedGame != null;

    public MainWindowViewModel(
        IAppInitializationService appInitializationService,
        ILogService logService,
        BannerViewModel bannerViewModel,
        GameListViewModel gameListViewModel,
        ModViewModel modViewModel
        )
    {
        _appInitializationService = appInitializationService;
        _logService = logService;
        BannerViewModel = bannerViewModel;
        GameListViewModel = gameListViewModel;
        ModViewModel = modViewModel;

        IsActive = true;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        _ = InitializeAsync();
    }

    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        OnPropertyChanged(nameof(IsGameSelected));

        if (!_suppressMessage)
            Messenger.Send(new SelectedGameChangedMessage(value));
    }

    public void Receive(SelectedGameChangedMessage message)
    {
        _suppressMessage = true;
        SelectedGame = message.Value;
        _suppressMessage = false;
    }

    private async Task InitializeAsync()
    {
        IsInitializing = true;

        var scanProgress = new Progress<GameScanProgressReportDto>(report =>
        {
            InitializationMessage = $"Scanning... Completed: {report.CompletedPlatform} " +
            $"({report.ScannersCompleted}/{report.TotalScanners})";
        });

        var result = await _appInitializationService.InitializeAsync(scanProgress);

        foreach (var item in result.Games)
        {
            var vm = new GameModViewModel(item.Game)
            {
                CompatibleRenoDXMod = item.CompatibleMod,
                CompatibleRenoDXGenericMod = item.CompatibleGenericMod,
            };

            GameListViewModel.Games.Add(vm);

            await _logService.LogInfoAsync(item.CompatibleMod is not null
                ? $"Compatible RenoDX game: {vm.Name}"
                : item.CompatibleGenericMod is not null
                    ? $"Compatible generic RenoDX game: {vm.Name}"
                    : $"Loaded game: {vm.Name}");
        }

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true);

        IsInitializing = false;
        InitializationMessage = string.Empty;
    }
}