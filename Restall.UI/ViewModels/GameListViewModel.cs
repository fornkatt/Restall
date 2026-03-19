using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Restall.Application.DTOs;
using Restall.Application.Interfaces;
using Restall.Application.UseCases;
using Restall.UI.Messages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Restall.UI.ViewModels;

public sealed partial class GameListViewModel : ViewModelBase, IRecipient<SelectedGameChangedMessage>
{
    private readonly IRefreshLibraryUseCase _fullRefreshLibrary;
    private readonly ILightRefreshLibraryUseCase _lightRefreshLibrary;
    private readonly ILogService _logService;

    public GameListViewModel(
        IRefreshLibraryUseCase refreshLibrary,
        ILightRefreshLibraryUseCase lightRefreshLibrary,
        ILogService logService
        )
    {
        _fullRefreshLibrary = refreshLibrary;
        _lightRefreshLibrary = lightRefreshLibrary;
        _logService = logService;
        IsActive = true;
    }

    private CancellationTokenSource _messageCts = new();
    
    private bool _suppressMessage;

    [ObservableProperty]
    private ObservableCollection<GameModViewModel> _games = [];

    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    [ObservableProperty] 
    private string? _scanMessage;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FullRefreshLibraryCommand))]
    [NotifyCanExecuteChangedFor(nameof(LightRefreshLibraryCommand))]
    private bool _isRefreshing;

    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        if (!_suppressMessage)
            Messenger.Send(new SelectedGameChangedMessage(value));
    }

    public void Receive(SelectedGameChangedMessage message)
    {
        _suppressMessage = true;
        SelectedGame = message.Value;
        _suppressMessage = false;
    }
    
    public void LoadGames(RefreshLibraryResultDto result)
    {
        Games.Clear();
        foreach (var item in result.Games)
        {
            Games.Add(new GameModViewModel(item.Game)
            {
                CompatibleRenoDXMod = item.CompatibleMod,
                CompatibleRenoDXGenericMod = item.CompatibleGenericMod
            });
        }

        SelectedGame = Games.FirstOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task FullRefreshLibraryAsync()
    {

        await ExecuteWithDelayedMessageAsync(async () =>
        {
            var result = await _fullRefreshLibrary.ExecuteFullRescanAsync();
            LoadGames(result);

        });
        
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true);
        
        
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task LightRefreshLibraryAsync()
    {
        var existingGames = Games.Select(g => g.GetGame()).ToList();

        await ExecuteWithDelayedMessageAsync(async () =>
        {
            var result = await _lightRefreshLibrary.ExecuteLightRescanAsync(existingGames);
            UpdateModCompatibility(result);
        });
    }

    private void UpdateModCompatibility(RefreshLibraryResultDto result)
    {
        var lookup = result.Games.ToDictionary(r => r.Game);

        foreach (var gameVm in Games)
        {
            if (!lookup.TryGetValue(gameVm.GetGame(), out var item)) continue;

            gameVm.CompatibleRenoDXMod = item.CompatibleMod;
            gameVm.CompatibleRenoDXGenericMod = item.CompatibleGenericMod;
            gameVm.NotifyGameStateChanged();
        }

        Messenger.Send(new WikiRefreshedMessage());
    }

    private async Task ExecuteWithDelayedMessageAsync(Func<Task> work)
    {
        await _messageCts.CancelAsync();
        _messageCts = new CancellationTokenSource();
        var token = _messageCts.Token;
        // Protects the scan operation
        try
        {
            ScanMessage = "Scanning...";
            IsRefreshing = true;
            await work();
            ScanMessage = "Completed!";
        }
        catch (Exception ex)
        {
            ScanMessage = "Error";
            await _logService.LogErrorAsync("An error occured during scanning", ex);
        }
        finally
        {
            IsRefreshing = false;
        }
        
        // Protects the UI timer
        try
        {
            await Task.Delay(2000, token);
            if (!token.IsCancellationRequested)
            {
                ScanMessage = string.Empty;
            }
        }
        catch(OperationCanceledException) { }
    }

    private bool CanRefresh() => !IsRefreshing;
}